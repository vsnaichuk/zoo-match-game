using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;

    public Item item
    {
        get => this._item;

        set
        {
            if (this._item == value) return;

            this._item = value;
            icon.sprite = this._item.sprite;
        }
    }

    public Image icon;

    public Button button;

    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null;
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y - 1] : null;
    public Tile Right => x < Board.Instance.Width - 1 ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Bottom => y < Board.Instance.Height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

    public Tile[] Neighbors => new[]
    {
        Left,
        Top,
        Right,
        Bottom,
    };

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this));
    }

    public List<Tile> GetConnectedTiles(List<Tile> exclude = null)
    {
        var result = new List<Tile> { this, };
        if (exclude == null)
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var n in Neighbors)
        {
            if (n == null || exclude.Contains(n) || n.item != item) continue;
            result.AddRange(n.GetConnectedTiles(exclude));
        }

        return result;

    }
}
