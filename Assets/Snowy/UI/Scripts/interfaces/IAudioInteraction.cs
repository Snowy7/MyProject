using UnityEngine;

namespace Snowy.UI.interfaces
{
    public interface IAudioInteraction
    {
        public AudioClip HoverSound { get; set; }
        public AudioClip ClickSound { get; set; }
    }
}