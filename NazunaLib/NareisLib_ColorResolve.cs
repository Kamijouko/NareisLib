using AlienRace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace NareisLib
{
    public class NareisLib_ColorResolve
    {
        public static Color ResolveColorFirst(AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.color;
            if (data.useBodyColor)
                return alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
            return pawn.story.HairColor;
        }
        public static Color ResolveColorFirst(Color color, AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.color;
            if (data.useBodyColor)
                return alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
            return color;
        }
        public static Color ResolveColorFirstOne(Color color, AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.color;
            if (data.useHairColor)
                return alienComp == null ? pawn.story.HairColor : alienComp.GetChannel("hair").first;
            return color;
        }
        public static Color ResolveColorFirstTwo(AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.color;
            if (data.useHairColor)
                return alienComp == null ? pawn.story.HairColor : alienComp.GetChannel("hair").first;
            if (data.useBodyColor)
                return alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
            return Color.white;
        }
        public static Color ResolveColorFirstTwo(Color color, AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.color;
            if (data.useHairColor)
                return alienComp == null ? pawn.story.HairColor : alienComp.GetChannel("hair").first;
            if (data.useBodyColor)
                return alienComp == null ? pawn.story.SkinColor : alienComp.GetChannel("skin").first;
            return color;
        }

        public static Color ResolveColorSecond(Color color, AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.colorTwo;
            if (data.useBodyColor)
                return alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
            return color;
        }
        public static Color ResolveColorSecondOne(Color color, AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.colorTwo;
            if (data.useHairColor)
                return alienComp == null ? Color.white : alienComp.GetChannel("hair").second;
            return color;
        }
        public static Color ResolveColorSecondTwo(AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.colorTwo;
            if (data.useHairColor)
                return alienComp == null ? Color.white : alienComp.GetChannel("hair").second;
            if (data.useBodyColor)
                return alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
            return Color.white;
        }
        public static Color ResolveColorSecondTwo(Color color, AlienPartGenerator.AlienComp alienComp, TextureLevels data, Pawn pawn)
        {
            if (data.useStaticColor)
                return data.colorTwo;
            if (data.useHairColor)
                return alienComp == null ? Color.white : alienComp.GetChannel("hair").second;
            if (data.useBodyColor)
                return alienComp == null ? Color.white : alienComp.GetChannel("skin").second;
            return color;
        }
    }
}
