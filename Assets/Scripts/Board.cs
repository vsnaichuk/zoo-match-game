using DG.Tweening;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public static Board Instance { get; private set; }

    public Row[] rows;

    // A 2D array of Tiles representing the game grid (rows and columns of tiles)
    public Tile[,] Tiles { get; private set; }

    public int Width => Tiles.GetLength(dimension: 0);
    public int Height => Tiles.GetLength(dimension: 1);

    private readonly List<Tile> _selection = new List<Tile>();

    private const float _moveDuration = 0.25f;
    private const float _scaleDuration = 0.2f;

    // Awake happens before Start
    private void Awake() => Instance = this;

    private void Start()
    {
        // Initialize the Tiles 2D array: determine the size based on the rows and the length of tiles in each row
        // rows.Max(row => row.tiles.Length) gives the maximum length of the tile arrays in each row, which will represent the number of columns.
        // rows.Length gives the number of rows.
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        // Populate the 2D Tiles array
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];
                tile.x = x;
                tile.y = y;
                tile.item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

                Tiles[x, y] = tile;
            }

        }

    }

    private void Update()
    {
        if (!Input.GetKey(KeyCode.A)) return;

        foreach (var t in Tiles[0, 0].GetConnectedTiles())
        {
            t.icon.transform.DOScale(1.25f, 0.25f).Play();
        }
    }

    public async void Select(Tile tile)
    {
        if (!_selection.Contains(tile)) _selection.Add(tile);
        if (_selection.Count < 2) return;

        Debug.Log(message: $"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]);

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);

        }

        _selection.Clear();
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon;
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform;
        var icon2Transform = icon2.transform;

        // Start swap animation
        var sequence = DOTween.Sequence();

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, _moveDuration).SetEase(Ease.OutBack))
                .Join(icon2Transform.DOMove(icon1Transform.position, _moveDuration).SetEase(Ease.OutBack));

        await sequence.Play()
                      .AsyncWaitForCompletion();

        // Update icon positions
        icon1Transform.SetParent(tile2.transform);
        icon2Transform.SetParent(tile1.transform);

        tile1.icon = icon2;
        tile2.icon = icon1;

        var tempTileItem = tile1.item;
        tile1.item = tile2.item;
        tile2.item = tempTileItem;
    }

    private bool CanPop()
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                    return true;

        return false;
    }

    private async void Pop()
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];
                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2) continue;

                // to animate the shrinking, scale down of the connected tiles
                var deflateSequence = DOTween.Sequence();
                foreach (var t in connectedTiles)
                    deflateSequence.Join(t.icon.transform.DOScale(Vector3.zero, _scaleDuration));

                await deflateSequence.Play()
                    .AsyncWaitForCompletion();

                // to animate the inflation, scaling to original size of the connected tiles
                var inflateSequence = DOTween.Sequence();
                foreach (var t in connectedTiles)
                {
                    t.item = ItemDatabase.Items[Random.Range(0, ItemDatabase.Items.Length)];

                    inflateSequence.Join(t.icon.transform.DOScale(Vector3.one, _scaleDuration));
                }

                await inflateSequence.Play()
                    .AsyncWaitForCompletion();
            }
    }
}
