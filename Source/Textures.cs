using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace BigRedButton {
    [StaticConstructorOnStartup]
    public static class Textures {
        private const string Prefix = Strings.ID + "/";

        public static readonly Texture2D Button = ContentFinder<Texture2D>.Get(Prefix + "Button");
    }
}
