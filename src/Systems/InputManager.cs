using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace LegacyOfTheShatteredCrown.Systems;

/// <summary>
/// Handles mouse input and converts screen positions to grid coordinates.
/// Tracks click states for selection logic.
/// </summary>
public class InputManager
{
    private MouseState _currentMouseState;
    private MouseState _previousMouseState;

    /// <summary>
    /// Current mouse position in screen pixels.
    /// </summary>
    public Vector2 MousePosition => _currentMouseState.Position.ToVector2();

    /// <summary>
    /// Current grid tile under the mouse cursor (null if outside grid).
    /// </summary>
    public Point? HoveredTile => Grid.PixelToGrid(MousePosition);

    /// <summary>
    /// True on the frame when left mouse button is first pressed.
    /// </summary>
    public bool LeftClickPressed =>
        _currentMouseState.LeftButton == ButtonState.Pressed &&
        _previousMouseState.LeftButton == ButtonState.Released;

    /// <summary>
    /// True on the frame when right mouse button is first pressed.
    /// </summary>
    public bool RightClickPressed =>
        _currentMouseState.RightButton == ButtonState.Pressed &&
        _previousMouseState.RightButton == ButtonState.Released;

    /// <summary>
    /// True while left mouse button is held down.
    /// </summary>
    public bool LeftClickHeld => _currentMouseState.LeftButton == ButtonState.Pressed;

    /// <summary>
    /// True while right mouse button is held down.
    /// </summary>
    public bool RightClickHeld => _currentMouseState.RightButton == ButtonState.Pressed;

    public InputManager()
    {
        _currentMouseState = Mouse.GetState();
        _previousMouseState = _currentMouseState;
    }

    /// <summary>
    /// Updates input state. Call once per frame at the start of Update.
    /// </summary>
    public void Update()
    {
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
    }

    /// <summary>
    /// Gets the grid tile that was clicked this frame (null if no valid click).
    /// </summary>
    public Point? GetClickedTile()
    {
        if (LeftClickPressed)
        {
            return HoveredTile;
        }
        return null;
    }
}

