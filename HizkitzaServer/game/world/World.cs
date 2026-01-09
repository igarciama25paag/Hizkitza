using HizkitzaServer.game.util;
using HizkitzaServer.game.world;

namespace HizkitzaServer.world;

using System;
using System.Collections.Generic;
using System.IO;

public class World
{
    private List<string> Map = [];
    public  List<string> Picture { get; } = [];
    private EntityMap EntityMap;
    public Clock Clock { get; }

    // Constructors //

    public World(string file)
    {
        using var sr = new StreamReader(new FileInfo(file).FullName);
        while (!sr.EndOfStream) Map.Add(sr.ReadLine());
        EntityMap = new EntityMap(this);
        Clock = new Clock();
    }

    // Getters and Setters //

    public List<string> GetMap() => Picture;

    // Object //

    public override string ToString()
    {
        var str = "";
        foreach (var s in Picture)
            str += s + "<br>";
        return str;
    }

    // Functions //

    public void DisplayEntities()
    {
        foreach (var entity in EntityMap.Entities)
            Display(entity.Look, entity.X, entity.Y);
    }

    private void Display(char look, int x, int y) => Picture[y] = PutCharInString(look, Picture[y], x);

    private string PutCharInString(char c, string t, int x) => t[..x] + c + t[(x + 1)..];

    public void PutColor()
    {
        const string close = "</font>";
        for (var y = Picture.Count - 1; y >= 0; y--)
        {
            Picture[y] += close;
            for (var x = Picture[y].Length - close.Length; x > 0; x--)
            {
                if (EntityMap.EntitiesIn(x, y).Count > 0)
                    // TODO color a entidades
                if (Picture[y][x] != Picture[y][x - 1])
                    Picture[y] =
                        Picture[y][..x] +
                        close + "<font color=" +
                        SymbolColor(Picture[y][x]) +
                        ">" +
                        Picture[y][x..];
            }

            Picture[y] = "<font color=" + SymbolColor(Picture[y][0]) + ">" + Picture[y];
        }
    }

    private string SymbolColor(char symbol)
    {
        return symbol switch
        {
            '@' => "yellow",
            'a' => "red",
            'o' => "green",
            _ => "white"
        };
    }

    public char? SymbolIn(int x, int y)
    {
        if (x < 0 || y < 0 ||
            x >= Map[x].Length ||
            y >= Map.Count)
            return null;
        return Map[y][x];
    }
}
