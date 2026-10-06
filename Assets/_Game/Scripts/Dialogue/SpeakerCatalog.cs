using UnityEngine;

namespace ThuyKieu.Dialogue
{
    public enum SpeakerStyle { Spoken, Narration, Inner, Spirit, Verse }

    /// <summary>Display data for Ink speaker ids. Unknown ids fall back to narration.</summary>
    public static class SpeakerCatalog
    {
        public struct Entry
        {
            public string DisplayName;
            public Color Color;
            public SpeakerStyle Style;
            public Entry(string name, Color color, SpeakerStyle style) { DisplayName = name; Color = color; Style = style; }
        }

        public static Entry Get(string id)
        {
            switch (id)
            {
                case "Kieu": return new Entry("Thúy Kiều", new Color(1f, 0.77f, 0.4f), SpeakerStyle.Spoken);
                case "MeKieu": return new Entry("Mẹ Kiều", new Color(0.86f, 0.78f, 0.66f), SpeakerStyle.Spoken);
                case "MaGiamSinh": return new Entry("Mã Giám Sinh", new Color(0.62f, 0.85f, 0.6f), SpeakerStyle.Spoken);
                case "TuyTung": return new Entry("Tùy tùng", new Color(0.7f, 0.74f, 0.8f), SpeakerStyle.Spoken);
                case "TuBa": return new Entry("Tú Bà", new Color(0.95f, 0.45f, 0.5f), SpeakerStyle.Spoken);
                case "MaKieu": return new Entry("Mã Kiều", new Color(0.78f, 0.62f, 0.95f), SpeakerStyle.Spoken);
                case "CoHong": return new Entry("Cô gái áo hồng", new Color(1f, 0.66f, 0.78f), SpeakerStyle.Spoken);
                case "CoLam": return new Entry("Cô gái áo lam", new Color(0.6f, 0.78f, 1f), SpeakerStyle.Spoken);
                case "NguoiCanh": return new Entry("Người canh", new Color(0.8f, 0.6f, 0.45f), SpeakerStyle.Spoken);
                case "NoiTam": return new Entry("Nội tâm", new Color(0.82f, 0.9f, 1f), SpeakerStyle.Inner);
                case "KieuThat": return new Entry("Thúy Kiều thật", new Color(1f, 0.84f, 0.45f), SpeakerStyle.Spirit);
                case "TrangSach": return new Entry("Truyện Kiều", new Color(0.93f, 0.86f, 0.7f), SpeakerStyle.Verse);
                default: return new Entry("Dẫn chuyện", new Color(0.85f, 0.85f, 0.85f), SpeakerStyle.Narration);
            }
        }
    }
}
