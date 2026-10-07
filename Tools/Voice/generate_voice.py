"""Generates voice clips for every line in voice_manifest.json with Microsoft Edge neural TTS.

Usage (from the Unity project folder, after exporting the manifest with
ThuyKieu > Dialogue > Export voice manifest):

    python Tools/Voice/generate_voice.py            # only lines without a clip yet
    python Tools/Voice/generate_voice.py --force    # regenerate everything (after changing VOICES)

Clips go to Assets/_Game/Audio/Voice/Resources/Voice/<key>.mp3, where VoicePlayer loads them.
Clips whose line no longer exists in the manifest are deleted. Requires: pip install edge-tts
"""
import argparse
import asyncio
import json
import pathlib
import re

import edge_tts

ROOT = pathlib.Path(__file__).resolve().parents[2]
MANIFEST = ROOT / "Tools" / "Voice" / "voice_manifest.json"
OUTPUT = ROOT / "Assets" / "_Game" / "Audio" / "Voice" / "Resources" / "Voice"

FEMALE = "vi-VN-HoaiMyNeural"
MALE = "vi-VN-NamMinhNeural"

# Only two Vietnamese neural voices exist, so characters are told apart by pitch and pace.
VOICES = {
    "Kieu":       (FEMALE, "-4%", "+0Hz"),
    "NoiTam":     (FEMALE, "+6%", "+3Hz"),    # the modern student thinking: quicker, lighter
    "KieuThat":   (FEMALE, "-14%", "-5Hz"),   # the real Kiều: slow and grave (reverb added in game)
    "MeKieu":     (FEMALE, "-12%", "-14Hz"),
    "TuBa":       (FEMALE, "-6%", "-9Hz"),
    "MaKieu":     (FEMALE, "-8%", "-4Hz"),
    "CoHong":     (FEMALE, "+6%", "+9Hz"),
    "CoLam":      (FEMALE, "-2%", "+4Hz"),
    "TrangSach":  (FEMALE, "-18%", "-2Hz"),   # verses of Truyện Kiều
    "MaGiamSinh": (MALE, "-4%", "+0Hz"),
    "TuyTung":    (MALE, "+4%", "-8Hz"),
    "NguoiCanh":  (MALE, "+2%", "-14Hz"),
    "Narrator":   (MALE, "-10%", "-6Hz"),
}


def spoken(text: str) -> str:
    # "Tú Bà: ..." — the dialogue box already shows who speaks; don't read the name aloud.
    text = re.sub(r"^[^:()\"“]{1,24}:\s*", "", text)
    # Leading stage directions such as "(thì thầm)" are acted, not read — unless the whole line is a thought.
    direction = re.match(r"^\(([^)]{1,30})\)\s*(.+)$", text)
    if direction:
        text = direction.group(2)
    text = re.sub(r"[“”\"]", "", text)
    text = re.sub(r"\s*—\s*", ", ", text).replace("…", "...")
    return text.strip()


async def synthesise(line, semaphore, force):
    target = OUTPUT / f"{line['key']}.mp3"
    if target.exists() and not force:
        return "skip"
    voice, rate, pitch = VOICES.get(line["speaker"], VOICES["Narrator"])
    async with semaphore:
        for attempt in range(4):
            try:
                await edge_tts.Communicate(spoken(line["text"]), voice, rate=rate, pitch=pitch).save(str(target))
                return "made"
            except Exception as error:  # network hiccups: retry with backoff
                if attempt == 3:
                    print(f"FAILED {line['key']} ({line['speaker']}): {error}")
                    return "fail"
                await asyncio.sleep(1.5 * (attempt + 1))


async def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true", help="regenerate clips that already exist")
    args = parser.parse_args()

    lines = json.loads(MANIFEST.read_text(encoding="utf-8"))
    OUTPUT.mkdir(parents=True, exist_ok=True)
    keys = {line["key"] for line in lines}
    for stale in OUTPUT.glob("*.mp3"):
        if stale.stem not in keys:
            stale.unlink()
            meta = stale.with_suffix(".mp3.meta")
            if meta.exists():
                meta.unlink()

    semaphore = asyncio.Semaphore(4)
    results = await asyncio.gather(*(synthesise(line, semaphore, args.force) for line in lines))
    print(f"{len(lines)} lines: {results.count('made')} generated, {results.count('skip')} kept, {results.count('fail')} failed")


if __name__ == "__main__":
    asyncio.run(main())
