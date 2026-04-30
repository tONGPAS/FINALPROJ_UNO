using NAudio.Wave;
using System.IO;
using System.Windows.Forms;

namespace UNOFinal
{
    public static class MusicManager
    {
        private static IWavePlayer waveOut;
        private static AudioFileReader audioFileReader;
        private static string currentMusic = "";
        private static IWavePlayer winnerPlayer;
        private static AudioFileReader winnerReader;

        private const string MENU_MUSIC = "menu_music.mp3";
        private const string GAME_MUSIC = "game_music.mp3";
        private const string WINNER_SOUND = "winner_sound.mp3";

        private static string GetSoundPath(string fileName)
        {
            return Path.Combine(Application.StartupPath,fileName);
        }

        public static void PlayMenuMusic()
        {
            PlayMusic(MENU_MUSIC);
        }

        public static void PlayGameMusic()
        {
            PlayMusic(GAME_MUSIC);
        }

        public static void PlayWinnerSound()
        {
            try
            {
                string path = GetSoundPath(WINNER_SOUND);
                if (System.IO.File.Exists(path))
                {
                    
                    StopWinnerSound();

                    winnerPlayer = new WaveOutEvent();
                    winnerReader = new AudioFileReader(path);
                    winnerPlayer.Init(winnerReader);
                    winnerPlayer.Play();
                }
            }
            catch { }
        }

        public static void StopWinnerSound()
        {
            try
            {
                winnerPlayer?.Stop();
                winnerPlayer?.Dispose();
                winnerPlayer = null;

                winnerReader?.Dispose();
                winnerReader = null;
            }
            catch { }
        }

        private static void PlayMusic(string musicFile)
        {
            if (currentMusic == musicFile) return;

            try
            {
                string path = GetSoundPath(musicFile);
                if (File.Exists(path))
                {
                    StopMusic();

                    waveOut = new WaveOutEvent();
                    audioFileReader = new AudioFileReader(path);
                    audioFileReader.Volume = 0.25f;
                    waveOut.Init(audioFileReader);
                    waveOut.Play();
                    currentMusic = musicFile;
                }
            }
            catch { }
        }

        public static void StopMusic()
        {
            waveOut?.Stop();
            waveOut?.Dispose();
            waveOut = null;

            audioFileReader?.Dispose();
            audioFileReader = null;

            currentMusic = "";
        }
    }
}