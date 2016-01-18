using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Media; // for SoundPlayer

namespace MaruBatsuGame
{
    // マスデータを表す列挙型。
    enum Masu
    {
        EMPTY,  // 空
        MARU,   // マル (○)
        BATSU   // バツ (×)
    };

    // 判定結果を表す列挙型。
    enum HanteiKekka
    {
        MARU_KACHI,     // マルの勝ち。 
        BATSU_KACHI,    // バツの勝ち。
        HIKIWAKE        // 引き分け。
    };

    // 盤クラス。
    class Board
    {
        // 列の長さ。
        public const int NUM_ROW = 3;
        // マスの総数。
        public const int NUM_MASU = NUM_ROW * NUM_ROW;
        // 並びデータ。
        public static int[][] narabi_data = new int[][] {
            // 横の並び。
            new int[]{1, 2, 3}, // #0
            new int[]{4, 5, 6}, // #1
            new int[]{7, 8, 9}, // #2
            // 縦の並び。
            new int[]{1, 4, 7}, // #3
            new int[]{2, 5, 8}, // #4
            new int[]{3, 6, 9}, // #5
            // 斜めの並び。
            new int[]{1, 5, 9}, // #6
            new int[]{3, 5, 7}  // #7
        };
        // ビンゴデータ。並びインデックスからマスへの辞書。
        // 例えば、4,5,6でマルが並んでいれば、bingo[1] == Masu.MARU になる。
        public Dictionary<int, Masu> bingo;
        // 数からマスへの辞書。
        protected Dictionary<int, Masu> dict_number_kara_masu;

        public Board()
        {
            // マスデータを作成する。
            dict_number_kara_masu = new Dictionary<int, Masu>();
            // ビンゴデータを作成する。
            bingo = new Dictionary<int, Masu>();
            // 初期化。
            init();
        }

        // 盤の初期化。
        public void init()
        {
            // マスデータを空にする。
            for (int i = 1; i <= Board.NUM_MASU; ++i)
            {
                dict_number_kara_masu[i] = Masu.EMPTY;
            }
            // ビンゴデータをクリアする。
            bingo.Clear();
        }

        // マスを取得する。
        public Masu get_masu(int number)
        {
            if (dict_number_kara_masu.ContainsKey(number))
            {
                return dict_number_kara_masu[number];
            }
            return Masu.EMPTY;
        }

        // マスをセットする。
        public void set_masu(int number, Masu masu)
        {
            dict_number_kara_masu[number] = masu;
        }

        // 判定。
        public HanteiKekka hantei()
        {
            bingo.Clear();

            HanteiKekka ret = HanteiKekka.HIKIWAKE;
            for (int i = 0; i < narabi_data.Length; ++i)
            {
                var entry = narabi_data[i];
                if (get_masu(entry[0]) == Masu.MARU &&
                    get_masu(entry[1]) == Masu.MARU &&
                    get_masu(entry[2]) == Masu.MARU)
                {
                    ret = HanteiKekka.MARU_KACHI;
                    bingo[i] = Masu.MARU;
                }
                if (get_masu(entry[0]) == Masu.BATSU &&
                    get_masu(entry[1]) == Masu.BATSU &&
                    get_masu(entry[2]) == Masu.BATSU)
                {
                    ret = HanteiKekka.BATSU_KACHI;
                    bingo[i] = Masu.BATSU;
                }
            }
            return ret;
        }

        // 並びの集計を行う関数。
        public bool narabi_shuukei(int narabi_index, out int maru, out int batsu)
        {
            int kara;
            kara = maru = batsu = 0;
            var narabi = narabi_data[narabi_index];
            for (int i = 0; i < NUM_ROW; ++i)
            {
                var masu = get_masu(narabi[i]);
                switch (masu)
                {
                    case Masu.EMPTY:
                        ++kara;
                        break;
                    case Masu.MARU:
                        ++maru;
                        break;
                    case Masu.BATSU:
                        ++batsu;
                        break;
                }
            }
            return kara == NUM_ROW;
        }
    }

    // メインフォーム。
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // 盤。
        Board board = new Board();
        // 数からPictureBoxへの辞書。
        Dictionary<int, PictureBox> dict_number_kara_pic;
        // PictureBoxから数への辞書。
        Dictionary<PictureBox, int> dict_pic_kara_number;
        // 音。
        SoundPlayer player_maru = new SoundPlayer(Properties.Resources.sound_maru);
        SoundPlayer player_batsu = new SoundPlayer(Properties.Resources.sound_batsu);
        SoundPlayer player_yatta = new SoundPlayer(Properties.Resources.sound_yatta);
        SoundPlayer player_zannen = new SoundPlayer(Properties.Resources.sound_zannen);
        // 点滅用のタイマー。
        System.Timers.Timer timer_blink;
        // コンピュータ待ちのタイマー。
        System.Timers.Timer timer_computer;

        private void Form1_Load(object sender, EventArgs e)
        {
            // 盤を初期化。
            board.init();

            // 数からPictureBoxへの辞書を作成する。
            dict_number_kara_pic = new Dictionary<int, PictureBox>();
            dict_number_kara_pic[1] = pictureBox1;
            dict_number_kara_pic[2] = pictureBox2;
            dict_number_kara_pic[3] = pictureBox3;
            dict_number_kara_pic[4] = pictureBox4;
            dict_number_kara_pic[5] = pictureBox5;
            dict_number_kara_pic[6] = pictureBox6;
            dict_number_kara_pic[7] = pictureBox7;
            dict_number_kara_pic[8] = pictureBox8;
            dict_number_kara_pic[9] = pictureBox9;

            // PictureBoxから数への辞書を作成する。
            dict_pic_kara_number = new Dictionary<PictureBox, int>();
            dict_pic_kara_number[pictureBox1] = 1;
            dict_pic_kara_number[pictureBox2] = 2;
            dict_pic_kara_number[pictureBox3] = 3;
            dict_pic_kara_number[pictureBox4] = 4;
            dict_pic_kara_number[pictureBox5] = 5;
            dict_pic_kara_number[pictureBox6] = 6;
            dict_pic_kara_number[pictureBox7] = 7;
            dict_pic_kara_number[pictureBox8] = 8;
            dict_pic_kara_number[pictureBox9] = 9;

            // 点滅用のタイマーを作成する。
            timer_blink = new System.Timers.Timer();
            timer_blink.Interval = 1000;

            // コンピュータ待ちのタイマーを作成する。
            timer_computer = new System.Timers.Timer();
            timer_computer.Interval = 2000;
        }

        // マスがクリックされた。
        void on_masu_click(int number)
        {
            // すでにマスが置かれているか確認。
            if (board.get_masu(number) != Masu.EMPTY)
            {
                return;
            }

            // 盤のマスを変える。
            board.set_masu(number, Masu.MARU);

            // マスの画像を変える。
            dict_number_kara_pic[number].Image = Properties.Resources.image_maru;

            // 判定する。
            var kekka = board.hantei();
            switch (kekka)
            {
                case HanteiKekka.MARU_KACHI:
                    player_yatta.Play();
                    break;
                case HanteiKekka.BATSU_KACHI:
                    player_zannen.Play();
                    break;
                case HanteiKekka.HIKIWAKE:
                    player_maru.Play();
                    break;
            }
        }

        // 「開始」ボタンがクリックされた。
        private void button1_Click(object sender, EventArgs e)
        {
            // 盤を初期化する。
            board.init();

            // PicutureBoxの画像をクリアする。
            for (int i = 1; i <= Board.NUM_MASU; ++i)
            {
                dict_number_kara_pic[i].Image = null;
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            on_masu_click(1);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            on_masu_click(2);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            on_masu_click(3);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            on_masu_click(4);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            on_masu_click(5);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            on_masu_click(6);
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            on_masu_click(7);
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            on_masu_click(8);
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            on_masu_click(9);
        }
    }
}
