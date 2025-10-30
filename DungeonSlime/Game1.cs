using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;

namespace DungeonSlime;

public enum Direction
{
    Down,
    DownLeft,
    DownRight,
    Up,
    UpLeft,
    UpRight,
    Left,
    Right
}

public class Game1 : Core
{
    private Texture2D _logo;
    private Texture2D _walkSpriteSheet;
    private Texture2D _idleSpriteSheet;
    
    // Animation variables
    private int _currentFrame = 0;
    private const int _totalFrames = 8;
    private double _animationTimer = 0;
    private const double _frameTime = 0.1; // 100ms per frame
    
    // Movement variables
    private Vector2 _playerPosition = new Vector2(100, 100);
    private const float MOVEMENT_SPEED = 120f; // Pixels per second
    private Direction _currentDirection = Direction.Down;
    private bool _isMoving = false;
    
    // Sprite and hitbox configuration
    // Frame size: 48x64, Sprite size: 16x32
    private const int HITBOX_WIDTH = 16;      // Matches sprite width
    private const int HITBOX_HEIGHT = 32;     // Matches sprite height
    private const int HITBOX_OFFSET_X = 16;   // Centers 16px sprite in 48px frame: (48-16)/2 = 16
    private const int HITBOX_OFFSET_Y = 16;   // Centers 32px sprite in 64px frame: (64-32)/2 = 16
    
    // Debug mode - press F1 to toggle hitbox visualization
    private bool _showDebugHitbox = true;
    private Texture2D _debugPixel;

    public Game1() : base("Dungeon Slime", 1280, 720, false)
    {

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
        
        // Create a 1x1 white pixel for debug drawing (after GraphicsDevice is initialized)
        _debugPixel = new Texture2D(GraphicsDevice, 1, 1);
        _debugPixel.SetData(new[] { Color.White });
    }

    protected override void LoadContent()
    {
        // TODO: use this.Content to load your game content here
        _logo = Content.Load<Texture2D>("images/mglogo");
        _walkSpriteSheet = Content.Load<Texture2D>("images/walk");
        _idleSpriteSheet = Content.Load<Texture2D>("images/Idle");
        
        base.LoadContent();
    }

    private KeyboardState _previousKeyboardState;
    
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        KeyboardState currentKeyboardState = Keyboard.GetState();
        
        // Toggle debug hitbox visualization with F1
        if (currentKeyboardState.IsKeyDown(Keys.F1) && !_previousKeyboardState.IsKeyDown(Keys.F1))
        {
            _showDebugHitbox = !_showDebugHitbox;
        }
        _previousKeyboardState = currentKeyboardState;

        // Handle 8-directional movement input (WASD and Arrow keys)
        Vector2 movementInput = Vector2.Zero;
        
        // Check vertical input
        if (currentKeyboardState.IsKeyDown(Keys.W) || currentKeyboardState.IsKeyDown(Keys.Up))
            movementInput.Y -= 1;
        if (currentKeyboardState.IsKeyDown(Keys.S) || currentKeyboardState.IsKeyDown(Keys.Down))
            movementInput.Y += 1;
            
        // Check horizontal input
        if (currentKeyboardState.IsKeyDown(Keys.A) || currentKeyboardState.IsKeyDown(Keys.Left))
            movementInput.X -= 1;
        if (currentKeyboardState.IsKeyDown(Keys.D) || currentKeyboardState.IsKeyDown(Keys.Right))
            movementInput.X += 1;
        
        // Determine if moving and update direction
        _isMoving = movementInput != Vector2.Zero;
        
        if (_isMoving)
        {
            // Normalize diagonal movement to prevent faster diagonal speed
            if (movementInput.Length() > 1)
                movementInput.Normalize();
            
            // Update player position
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _playerPosition += movementInput * MOVEMENT_SPEED * deltaTime;
            
            // Clamp player position to window bounds using hitbox dimensions
            const int spriteScale = 4;
            int hitboxScreenWidth = HITBOX_WIDTH * spriteScale;       // 16 * 4 = 64
            int hitboxScreenHeight = HITBOX_HEIGHT * spriteScale;     // 32 * 4 = 128
            int hitboxOffsetXScaled = HITBOX_OFFSET_X * spriteScale;  // 16 * 4 = 64
            int hitboxOffsetYScaled = HITBOX_OFFSET_Y * spriteScale;  // 16 * 4 = 64
            const int yShift = 16 * spriteScale;                      // 16 source pixels = 64 screen pixels
            
            // Reduce bounds by 100% of hitbox width and 50% of hitbox height
            // Y axis shifted to align hitbox properly with screen bounds
            float minX = -hitboxOffsetXScaled;
            float maxX = Graphics.PreferredBackBufferWidth - hitboxOffsetXScaled - hitboxScreenWidth;
            float minY = -hitboxOffsetYScaled + (hitboxScreenHeight * 0.5f) - yShift;
            float maxY = Graphics.PreferredBackBufferHeight - hitboxOffsetYScaled - (hitboxScreenHeight * 0.5f) - yShift;
            
            _playerPosition.X = MathHelper.Clamp(_playerPosition.X, minX, maxX);
            _playerPosition.Y = MathHelper.Clamp(_playerPosition.Y, minY, maxY);
            
            // Determine direction based on input (8 directions)
            _currentDirection = GetDirectionFromInput(movementInput);
        }

        // Update animation (plays both when moving and idle)
        _animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_animationTimer >= _frameTime)
        {
            _animationTimer -= _frameTime;
            _currentFrame = (_currentFrame + 1) % _totalFrames;
        }

        base.Update(gameTime);
    }
    
    private Direction GetDirectionFromInput(Vector2 input)
    {
        // Determine 8-directional facing based on input
        if (input.Y < 0) // Moving up
        {
            if (input.X < 0) return Direction.UpLeft;
            if (input.X > 0) return Direction.UpRight;
            return Direction.Up;
        }
        else if (input.Y > 0) // Moving down
        {
            if (input.X < 0) return Direction.DownLeft;
            if (input.X > 0) return Direction.DownRight;
            return Direction.Down;
        }
        else // Moving horizontally only
        {
            if (input.X < 0) return Direction.Left;
            if (input.X > 0) return Direction.Right;
        }
        
        return _currentDirection; // Keep current direction if no input
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the sprite batch with PointClamp for crisp pixel art scaling
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the logo texture
        // SpriteBatch.Draw(_logo, Vector2.Zero, Color.White);

        // Draw animated character with directional sprite
        int frameWidth = 48;
        int frameHeight = 64;
        int frameSpacing = 48; // Horizontal spacing between frames
        int spriteScale = 4;
        
        // Get sprite row and flip based on direction
        (int spriteRow, bool flipHorizontal) = GetSpriteRowAndFlip(_currentDirection);
        
        // Calculate source rectangle based on current frame and direction
        Rectangle sourceRectangle = new Rectangle(
            _currentFrame * frameSpacing,
            spriteRow * frameHeight,
            frameWidth,
            frameHeight
        );
        
        // Calculate screen position and destination rectangle
        int spriteX = (int)_playerPosition.X;
        int spriteY = (int)_playerPosition.Y;
        Rectangle destinationRectangle = new Rectangle(
            spriteX, 
            spriteY, 
            frameWidth * spriteScale, 
            frameHeight * spriteScale
        );
        
        // Draw sprite with optional horizontal flip
        // Choose sprite sheet based on movement state
        Texture2D currentSpriteSheet = _isMoving ? _walkSpriteSheet : _idleSpriteSheet;
        SpriteEffects effect = flipHorizontal ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        SpriteBatch.Draw(currentSpriteSheet, destinationRectangle, sourceRectangle, Color.White, 0f, Vector2.Zero, effect, 0f);
        
        // Calculate hitbox position (scaled to match sprite)
        Rectangle hitbox = new Rectangle(
            spriteX + (HITBOX_OFFSET_X * spriteScale),
            spriteY + (HITBOX_OFFSET_Y * spriteScale),
            HITBOX_WIDTH * spriteScale,
            HITBOX_HEIGHT * spriteScale
        );
        
        // Draw hitbox outline for debugging
        if (_showDebugHitbox)
        {
            DrawRectangle(hitbox, Color.Red * 0.5f, 2);
        }

        // Always end the sprite batch when finished.
        SpriteBatch.End();

        base.Draw(gameTime);
    }
    
    private (int row, bool flip) GetSpriteRowAndFlip(Direction direction)
    {
        // Map directions to sprite sheet rows
        // Rows: 0=Down, 1=DownLeft, 2=UpLeft, 3=Up, 4=UpRight, 5=DownRight
        // We use horizontal flipping for pure Left/Right directions
        return direction switch
        {
            Direction.Down => (0, false),
            Direction.DownLeft => (1, false),
            Direction.DownRight => (5, false),
            Direction.Up => (3, false),
            Direction.UpLeft => (2, false),
            Direction.UpRight => (4, false),
            Direction.Left => (1, false),      // Use DownLeft animation for pure left
            Direction.Right => (5, false),     // Use DownRight animation for pure right
            _ => (0, false)
        };
    }
    
    // Helper method to draw rectangle outlines
    private void DrawRectangle(Rectangle rect, Color color, int lineWidth)
    {
        // Top
        SpriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, rect.Width, lineWidth), color);
        // Bottom
        SpriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Bottom - lineWidth, rect.Width, lineWidth), color);
        // Left
        SpriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, lineWidth, rect.Height), color);
        // Right
        SpriteBatch.Draw(_debugPixel, new Rectangle(rect.Right - lineWidth, rect.Top, lineWidth, rect.Height), color);
    }
}
