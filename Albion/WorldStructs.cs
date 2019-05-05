using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    public struct Cluster
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public ZoneType Type { get; set; }
        public Vector2 WorldPos { get; set; }
        public List<Connection> Connections { get; set; }
    }

    public struct Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    //I hate repeating this struct but I don't want to include the array above and I can't use an interface :(
    public struct Connection
    {
        public string ID { get; set; }
    }

    public enum ZoneType
    {
        OPENPVP_RED,
        GUILDISLAND,
        PLAYERCITY_SAFEAREA,
        PLAYERCITY_SAFEAREA_NOFURNITURE,
        OPENPVP_BLACK_3,
        OPENPVP_BLACK_2,
        OPENPVP_BLACK_1,
        OPENPVP_YELLOW,
        PLAYERISLAND,
        STARTINGCITY,
        STARTAREA,
        HARDCORE_EXPEDITION_STANDARD,
        T6_EXPEDITION_STANDARD,
        T5_EXPEDITION_STANDARD,
        T4_EXPEDITION_STANDARD,
        T3_EXPEDITION_STANDARD,
        HARDCORE_EXPEDITION_SURFACE,
        T6_EXPEDITION_SURFACE,
        T5_EXPEDITION_SURFACE,
        T4_EXPEDITION_SURFACE,
        SAFEAREA,
        OPENPVP_T5RED,
        TESTSTARTAREA,
        PASSAGE_RED,
        PASSAGE_BLACK,
        PLAYERCITY_BLACK,
        PLAYERCITY_BLACK_NOFURNITURE,
        DUNGEON_SAFEAREA_SANDBOX_SOLO,
        DUNGEON_YELLOW,
        DUNGEON_RED,
        DUNGEON_BLACK,
        DUNGEON_SAFEAREA,
        DUNGEON_RED_SANDBOX_SOLO,
        DUNGEON_HELL_GREEN,
        DUNGEON_HELL_YELLOW,
        DUNGEON_HELL,
        ARENA_STANDARD,
        ARENA_CUSTOM,
        ARENA_CRYSTAL,
        TUTORIAL
    }
}
