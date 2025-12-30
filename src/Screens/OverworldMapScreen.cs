using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegacyOfTheShatteredCrown.Data;

namespace LegacyOfTheShatteredCrown.Screens;

/// <summary>
/// The overworld/campaign map screen showing provinces as nodes.
/// Players can move between adjacent provinces and initiate battles.
/// </summary>
public class OverworldMapScreen
{
    private readonly Texture2D _pixelTexture;
    private readonly SpriteFont? _font;
    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private CampaignMap _campaignMap;
    private MouseState _previousMouseState;

    // Map rendering area
    private readonly Rectangle _mapArea;
    private const int MapPadding = 60;
    private const int NodeRadius = 24;
    private const int ConnectionWidth = 3;

    // Interaction state
    private string? _hoveredProvinceId;
    private Province? _selectedProvince;

    /// <summary>
    /// Fired when player initiates battle at a hostile province.
    /// </summary>
    public event Action<Province>? OnBattleStart;

    /// <summary>
    /// Fired when player moves to a liberated province.
    /// </summary>
    public event Action<Province>? OnProvinceEntered;

    public OverworldMapScreen(GraphicsDevice graphicsDevice, SpriteFont? font, int screenWidth, int screenHeight)
    {
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
        _font = font;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _mapArea = new Rectangle(
            MapPadding,
            MapPadding + 40,
            screenWidth - MapPadding * 2,
            screenHeight - MapPadding * 2 - 80
        );

        _campaignMap = CampaignMap.CreateDefaultMap();
    }

    /// <summary>
    /// Gets the current campaign map state.
    /// </summary>
    public CampaignMap CampaignMap => _campaignMap;

    public void Update()
    {
        var mouseState = Mouse.GetState();
        var mousePos = new Vector2(mouseState.X, mouseState.Y);

        // Check province hover
        _hoveredProvinceId = null;
        foreach (var province in _campaignMap.Provinces.Values)
        {
            var nodePos = GetNodeScreenPosition(province);
            float distance = Vector2.Distance(mousePos, nodePos);
            if (distance <= NodeRadius + 4)
            {
                _hoveredProvinceId = province.Id;
                break;
            }
        }

        // Handle clicks
        if (mouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            if (_hoveredProvinceId != null)
            {
                HandleProvinceClick(_hoveredProvinceId);
            }
            else
            {
                _selectedProvince = null;
            }
        }

        _previousMouseState = mouseState;
    }

    private void HandleProvinceClick(string provinceId)
    {
        var province = _campaignMap.Provinces[provinceId];

        // If clicking current province, just select it
        if (province.State == ProvinceState.Current)
        {
            _selectedProvince = province;
            return;
        }

        // Check if province is adjacent to current
        if (!_campaignMap.CanTravelTo(provinceId))
        {
            // Not adjacent - just select for info display
            _selectedProvince = province;
            return;
        }

        // Province is adjacent and can be traveled to
        if (province.State == ProvinceState.Hostile)
        {
            // Initiate battle
            _selectedProvince = province;
            OnBattleStart?.Invoke(province);
        }
        else if (province.State == ProvinceState.Liberated)
        {
            // Move to liberated province
            _campaignMap.SetCurrentProvince(provinceId);
            _selectedProvince = province;
            OnProvinceEntered?.Invoke(province);
        }
    }

    /// <summary>
    /// Called when a battle is won to liberate the province.
    /// </summary>
    public void OnBattleWon(string provinceId)
    {
        _campaignMap.LiberateProvince(provinceId);
        _campaignMap.SetCurrentProvince(provinceId);
    }

    /// <summary>
    /// Converts province map position (0-1) to screen coordinates.
    /// </summary>
    private Vector2 GetNodeScreenPosition(Province province)
    {
        return new Vector2(
            _mapArea.X + province.MapPosition.X * _mapArea.Width,
            _mapArea.Y + province.MapPosition.Y * _mapArea.Height
        );
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        // Background
        spriteBatch.Draw(_pixelTexture, new Rectangle(0, 0, _screenWidth, _screenHeight), new Color(20, 25, 35));

        // Title
        DrawTitle(spriteBatch);

        // Draw map background
        DrawMapBackground(spriteBatch);

        // Draw connections between provinces
        DrawConnections(spriteBatch);

        // Draw province nodes
        DrawProvinces(spriteBatch);

        // Draw realm legend
        DrawRealmLegend(spriteBatch);

        // Draw selected province info
        DrawProvinceInfo(spriteBatch);

        // Draw instructions
        DrawInstructions(spriteBatch);
    }

    private void DrawTitle(SpriteBatch spriteBatch)
    {
        string title = "CAMPAIGN MAP";

        if (_font != null)
        {
            var titleSize = _font.MeasureString(title);
            spriteBatch.DrawString(_font, title,
                new Vector2((_screenWidth - titleSize.X) / 2, 15),
                new Color(220, 180, 100));
        }
    }

    private void DrawMapBackground(SpriteBatch spriteBatch)
    {
        // Outer border
        var borderRect = new Rectangle(
            _mapArea.X - 4,
            _mapArea.Y - 4,
            _mapArea.Width + 8,
            _mapArea.Height + 8
        );
        spriteBatch.Draw(_pixelTexture, borderRect, new Color(60, 60, 80));

        // Map background
        spriteBatch.Draw(_pixelTexture, _mapArea, new Color(30, 35, 45));
    }

    private void DrawConnections(SpriteBatch spriteBatch)
    {
        var drawnConnections = new HashSet<string>();

        foreach (var province in _campaignMap.Provinces.Values)
        {
            var fromPos = GetNodeScreenPosition(province);

            foreach (var adjacentId in province.AdjacentProvinceIds)
            {
                // Avoid drawing same connection twice
                string connectionKey = string.Compare(province.Id, adjacentId) < 0
                    ? $"{province.Id}-{adjacentId}"
                    : $"{adjacentId}-{province.Id}";

                if (drawnConnections.Contains(connectionKey))
                    continue;

                drawnConnections.Add(connectionKey);

                if (!_campaignMap.Provinces.TryGetValue(adjacentId, out var adjacentProvince))
                    continue;

                var toPos = GetNodeScreenPosition(adjacentProvince);

                // Determine connection color based on accessibility
                var canTravel = _campaignMap.CanTravelTo(adjacentId) ||
                               (adjacentProvince.State == ProvinceState.Current);
                var lineColor = canTravel
                    ? new Color(80, 100, 80)
                    : new Color(50, 50, 60);

                DrawLine(spriteBatch, fromPos, toPos, lineColor, ConnectionWidth);
            }
        }
    }

    private void DrawProvinces(SpriteBatch spriteBatch)
    {
        foreach (var province in _campaignMap.Provinces.Values)
        {
            DrawProvinceNode(spriteBatch, province);
        }
    }

    private void DrawProvinceNode(SpriteBatch spriteBatch, Province province)
    {
        var pos = GetNodeScreenPosition(province);
        var realm = _campaignMap.Realms.TryGetValue(province.RealmId, out var r) ? r : null;
        bool isHovered = _hoveredProvinceId == province.Id;
        bool isSelected = _selectedProvince?.Id == province.Id;
        bool isAdjacent = _campaignMap.CanTravelTo(province.Id);

        // Determine colors based on state
        Color nodeColor;
        Color borderColor;

        switch (province.State)
        {
            case ProvinceState.Current:
                nodeColor = new Color(100, 200, 100);
                borderColor = new Color(150, 255, 150);
                break;
            case ProvinceState.Liberated:
                nodeColor = realm?.Color ?? new Color(80, 120, 80);
                borderColor = new Color(120, 180, 120);
                break;
            case ProvinceState.Hostile:
            default:
                nodeColor = new Color(180, 80, 80);
                borderColor = isAdjacent ? new Color(220, 120, 80) : new Color(120, 60, 60);
                break;
        }

        // Highlight on hover
        if (isHovered)
        {
            borderColor = new Color(255, 255, 200);
        }

        // Draw outer glow for selected
        if (isSelected)
        {
            DrawCircle(spriteBatch, pos, NodeRadius + 8, new Color(255, 220, 100, 100));
        }

        // Draw adjacent indicator
        if (isAdjacent && province.State == ProvinceState.Hostile)
        {
            DrawCircle(spriteBatch, pos, NodeRadius + 5, new Color(220, 180, 80, 150));
        }

        // Draw border circle
        DrawCircle(spriteBatch, pos, NodeRadius + 3, borderColor);

        // Draw main node
        DrawCircle(spriteBatch, pos, NodeRadius, nodeColor);

        // Draw inner detail
        if (province.State == ProvinceState.Current)
        {
            // Player marker
            DrawCircle(spriteBatch, pos, 8, Color.White);
        }
        else if (province.State == ProvinceState.Hostile)
        {
            // Enemy indicator
            DrawCircle(spriteBatch, pos, 6, new Color(50, 20, 20));
        }

        // Draw difficulty pips for hostile provinces
        if (province.State == ProvinceState.Hostile && province.Difficulty > 0)
        {
            DrawDifficultyPips(spriteBatch, pos, province.Difficulty);
        }

        // Draw province name
        if (_font != null && (isHovered || isSelected))
        {
            var nameSize = _font.MeasureString(province.Name);
            var namePos = new Vector2(pos.X - nameSize.X / 2, pos.Y - NodeRadius - 20);

            // Background for readability
            var bgRect = new Rectangle(
                (int)namePos.X - 4,
                (int)namePos.Y - 2,
                (int)nameSize.X + 8,
                (int)nameSize.Y + 4
            );
            spriteBatch.Draw(_pixelTexture, bgRect, new Color(0, 0, 0, 180));

            spriteBatch.DrawString(_font, province.Name, namePos, Color.White);
        }
    }

    private void DrawDifficultyPips(SpriteBatch spriteBatch, Vector2 center, int difficulty)
    {
        int pipSize = 4;
        int spacing = 6;
        int totalWidth = difficulty * spacing - (spacing - pipSize);
        float startX = center.X - totalWidth / 2f;
        float y = center.Y + NodeRadius + 8;

        for (int i = 0; i < difficulty; i++)
        {
            var pipRect = new Rectangle(
                (int)(startX + i * spacing),
                (int)y,
                pipSize,
                pipSize
            );
            spriteBatch.Draw(_pixelTexture, pipRect, new Color(200, 60, 60));
        }
    }

    private void DrawRealmLegend(SpriteBatch spriteBatch)
    {
        if (_font == null) return;

        int startY = _screenHeight - 70;
        int x = 20;

        foreach (var realm in _campaignMap.Realms.Values)
        {
            bool isLiberated = _campaignMap.IsRealmLiberated(realm.Id);

            // Color swatch
            var swatchRect = new Rectangle(x, startY, 12, 12);
            spriteBatch.Draw(_pixelTexture, swatchRect, realm.Color);

            if (isLiberated)
            {
                // Checkmark overlay
                spriteBatch.Draw(_pixelTexture,
                    new Rectangle(x + 3, startY + 3, 6, 6),
                    new Color(100, 255, 100));
            }

            // Realm name (shortened)
            string name = realm.Name.Length > 10 ? realm.Name.Substring(0, 8) + ".." : realm.Name;
            var color = isLiberated ? new Color(100, 200, 100) : new Color(150, 150, 160);
            spriteBatch.DrawString(_font, name, new Vector2(x + 16, startY - 2), color);

            x += 110;
        }
    }

    private void DrawProvinceInfo(SpriteBatch spriteBatch)
    {
        if (_font == null) return;

        var province = _selectedProvince ?? (_hoveredProvinceId != null ? _campaignMap.Provinces[_hoveredProvinceId] : null);
        if (province == null) return;

        int panelWidth = 200;
        int panelHeight = 120;
        int x = _screenWidth - panelWidth - 20;
        int y = 60;

        // Panel background
        var panelRect = new Rectangle(x, y, panelWidth, panelHeight);
        spriteBatch.Draw(_pixelTexture, panelRect, new Color(40, 40, 50, 230));

        // Border
        DrawRectangleBorder(spriteBatch, panelRect, new Color(80, 80, 100), 2);

        // Province name
        spriteBatch.DrawString(_font, province.Name, new Vector2(x + 10, y + 10), Color.White);

        // Realm
        var realm = _campaignMap.Realms.TryGetValue(province.RealmId, out var r) ? r : null;
        if (realm != null)
        {
            spriteBatch.DrawString(_font, realm.HouseAffiliation,
                new Vector2(x + 10, y + 30), realm.Color);
        }

        // State
        string stateText = province.State switch
        {
            ProvinceState.Current => "You are here",
            ProvinceState.Liberated => "Liberated",
            ProvinceState.Hostile => $"Hostile (Difficulty: {province.Difficulty})",
            _ => ""
        };
        var stateColor = province.State switch
        {
            ProvinceState.Current => new Color(100, 200, 100),
            ProvinceState.Liberated => new Color(100, 180, 100),
            ProvinceState.Hostile => new Color(200, 100, 100),
            _ => Color.Gray
        };
        spriteBatch.DrawString(_font, stateText, new Vector2(x + 10, y + 55), stateColor);

        // Action hint
        if (_campaignMap.CanTravelTo(province.Id))
        {
            string action = province.State == ProvinceState.Hostile
                ? "Click to attack!"
                : "Click to travel";
            spriteBatch.DrawString(_font, action,
                new Vector2(x + 10, y + 80),
                new Color(220, 180, 100));
        }
        else if (province.State != ProvinceState.Current)
        {
            spriteBatch.DrawString(_font, "(Not adjacent)",
                new Vector2(x + 10, y + 80),
                new Color(100, 100, 110));
        }
    }

    private void DrawInstructions(SpriteBatch spriteBatch)
    {
        if (_font == null) return;

        string instructions = "Click adjacent provinces to travel or attack  |  ESC: Return to menu";
        var size = _font.MeasureString(instructions);

        spriteBatch.DrawString(_font, instructions,
            new Vector2((_screenWidth - size.X) / 2, _screenHeight - 30),
            new Color(100, 100, 110));
    }

    private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color)
    {
        // Approximate circle with a filled rectangle (for simplicity)
        // In a real implementation, you'd use a circle texture or draw triangles
        var rect = new Rectangle(
            (int)(center.X - radius),
            (int)(center.Y - radius),
            (int)(radius * 2),
            (int)(radius * 2)
        );
        spriteBatch.Draw(_pixelTexture, rect, color);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        float length = edge.Length();

        spriteBatch.Draw(_pixelTexture,
            new Rectangle((int)start.X, (int)start.Y, (int)length, thickness),
            null,
            color,
            angle,
            Vector2.Zero,
            SpriteEffects.None,
            0);
    }

    private void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle rect, Color color, int thickness)
    {
        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, rect.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y + rect.Height - thickness, rect.Width, thickness), color);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X, rect.Y, thickness, rect.Height), color);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(rect.X + rect.Width - thickness, rect.Y, thickness, rect.Height), color);
    }

    public void Dispose()
    {
        _pixelTexture?.Dispose();
    }
}
