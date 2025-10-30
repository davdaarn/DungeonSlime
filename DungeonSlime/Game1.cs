using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameLibrary;

namespace DungeonSlime;

public class Game1 : Core
{
    private Texture2D _logo;
    private Texture2D _walkSpriteSheet;
    
    // Animation variables
    private int _currentFrame = 0;
    private const int _totalFrames = 8;
    private double _animationTimer = 0;
    private const double _frameTime = 0.1; // 100ms per frame
    
    // Hitbox configuration (separate from visual sprite size)
    private const int HITBOX_WIDTH = 16;      // Actual collision width (reduced 50%)
    private const int HITBOX_HEIGHT = 20;     // Actual collision height (reduced 50%)
    private const int HITBOX_OFFSET_X = 16;   // Offset from left of frame (centered horizontally)
    private const int HITBOX_OFFSET_Y = 22;   // Offset from top of frame (centered on body)
    
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
        
        base.LoadContent();
    }

    private KeyboardState _previousKeyboardState;
    
    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // Toggle debug hitbox visualization with F1
        KeyboardState currentKeyboardState = Keyboard.GetState();
        if (currentKeyboardState.IsKeyDown(Keys.F1) && !_previousKeyboardState.IsKeyDown(Keys.F1))
        {
            _showDebugHitbox = !_showDebugHitbox;
        }
        _previousKeyboardState = currentKeyboardState;

        // Update walk animation
        _animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_animationTimer >= _frameTime)
        {
            _animationTimer -= _frameTime;
            _currentFrame = (_currentFrame + 1) % _totalFrames;
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // Begin the sprite batch with PointClamp for crisp pixel art scaling
        SpriteBatch.Begin(samplerState: SamplerState.PointClamp);

        // Draw the logo texture
        // SpriteBatch.Draw(_logo, Vector2.Zero, Color.White);

        // Draw animated walk cycle (8 frames)
        // Calculate source rectangle based on current frame
        int frameWidth = 48;
        int frameHeight = 64;
        int frameSpacing = 48; // Horizontal spacing between frames
        int startX = 0;
        int startY = 0; // Start at top of sprite sheet
        
        Rectangle sourceRectangle = new Rectangle(
            startX + (_currentFrame * frameSpacing),
            startY,
            frameWidth,
            frameHeight
        );
        
        // Scale 4x, maintaining aspect ratio (48x64 becomes 192x256)
        int spriteX = 100;
        int spriteY = 100;
        int spriteScale = 4;
        Rectangle destinationRectangle = new Rectangle(spriteX, spriteY, frameWidth * spriteScale, frameHeight * spriteScale);
        SpriteBatch.Draw(_walkSpriteSheet, destinationRectangle, sourceRectangle, Color.White);
        
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
