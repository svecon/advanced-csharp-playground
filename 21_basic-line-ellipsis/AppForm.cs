using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Globalization;

namespace BasicLineEllipsis {
    public partial class AppForm : Form {
        private const string Ellipsis = "...";

        private Font eventFont = new Font("Arial", 10);

        public AppForm()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawRectangle(Pens.Black, 20, 20, 70, 500);

            string[] strings = {
                "Hello",
                "Hello world!",
                "ABCDEFGHIJKLMN",
                "iiiiiiiiXXXXXX",
                "Koupit řepu a mléko".Normalize(NormalizationForm.FormD),
                "ě\u0301ščřžýáíúůĚŠČŘŽÝÁÍÉ".Normalize(NormalizationForm.FormD),
                "ěščř\u0301\u0335\u0316žýáíúůĚŠČŘŽÝÁÍÉ".Normalize(NormalizationForm.FormD),
                "ěščř\u0301\u0335\u0316žýáíúůĚŠČŘŽÝ\u0336\u0317ÁÍÉ".Normalize(NormalizationForm.FormD),
				"ěšč\U0001F608\u0301\u0316žýáíúůĚŠČŘŽÝ\u0336\u0317ÁÍÉ".Normalize(NormalizationForm.FormD)
			};

            using (var w = new System.IO.StreamWriter("out.txt"))
            {
                for (int i = 0; i < strings.Length; i++)
                {
                    DrawString(g, strings[i], 20, 30 + i * 30, 70, w);
                }
            }
        }

        private void DrawString(Graphics g, string input, float x, float y, float width, System.IO.TextWriter writer)
        {
            string prefix, postfix;
            float prefixWidth, postfixWidth;

            SplitForEllipsis(g, input, width, out prefix, out postfix);

            writer.WriteLine("|{0}|{1}|", prefix, postfix);

            g.DrawString(prefix, eventFont, Brushes.Black, x, y, StringFormat.GenericTypographic);

            prefixWidth = stringWidth(g, prefix);
            postfixWidth = stringWidth(g, postfix);

            if (input != prefix)
            {
                float freeSpace = width - prefixWidth - postfixWidth - stringWidth(g, Ellipsis);
                g.DrawString(Ellipsis, eventFont, Brushes.Black, x + prefixWidth + freeSpace / 2, y, StringFormat.GenericTypographic);
            }

            if (postfix == null)
                return;

            g.DrawString(postfix, eventFont, Brushes.Black, x + width - postfixWidth, y, StringFormat.GenericTypographic);
        }

        private void SplitForEllipsis(Graphics g, string input, float maxWidth, out string prefix, out string postfix)
        {
            prefix = null;
            postfix = null;

            if (stringWidth(g, input) <= maxWidth)
            {
                prefix = input;
                return;
            }

            StringSide currentSide = StringSide.Prefix;
            int overflow = 0; // markering which sides are overflowed

            while (overflow < StringSidesSum && input != null)
            {
                string prevPrefix = prefix;
                string prevPostfix = postfix;

                incrementOneSide(ref prefix, ref postfix, ref input, currentSide);

                if (stringWidth(g, prefix + Ellipsis + postfix) > maxWidth)
                {
                    // revert back because width is exceeded
                    prefix = prevPrefix;
                    postfix = prevPostfix;

                    overflow |= (int)currentSide; // mark currentSide as overflowed
                    flipSide(ref currentSide); // continue on the other side (unless also overflowed)
                }

                if (overflow == 0) // both sides can still be added to -> flip currentSide
                    // METHOD 1: ADD ONE CHAR TO PREFIX, ONE TO POSTFIX
                    flipSide(ref currentSide);

                    // METHOD 2: ADD CHAR TO THE NARROWER SIDE
                    //if (stringWidth(g, prefix) > stringWidth(g, postfix))
                    //    currentSide = StringSide.Postfix;
                    //else
                    //    currentSide = StringSide.Prefix;
            }
        }

        private void flipSide(ref StringSide side)
        {
            side = (StringSide)((int)side ^ StringSidesSum);
        }

        enum StringSide { Prefix = 1, Postfix = 2 };
        const int StringSidesSum = (int)StringSide.Prefix + (int)StringSide.Postfix;

        private void incrementOneSide(ref string prefix, ref string postfix, ref string input, StringSide side)
        {
            if (input == null)
                return;

            // get indexes where normal characters start
            int[] textElements = StringInfo.ParseCombiningCharacters(input);

            if (textElements.Length < 2) // only one character remains -> append him
            {
                if (side == StringSide.Prefix)
                    prefix += input;
                else
                    postfix += input;

                input = null;
                return;
            }

            // get one complete character either from the begining or the end of the input
            // after that trim input by that character
            int startIndex = side == StringSide.Prefix ? 0 : textElements[textElements.Length - 1];
            int endIndex = side == StringSide.Prefix ? textElements[1] : input.Length;

            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < endIndex; i++)
                sb.Append(input[i]);

            if (side == StringSide.Prefix)
            {
                prefix = prefix + sb;
                input = input.Substring(textElements[1]);
            }
            else // if (side == StringSide.Postfix)
            {
                postfix = sb + postfix;
                input = input.Substring(0, textElements[textElements.Length - 1]);
            }
        }

        private float stringWidth(Graphics g, string input)
        {
            // just a shortcut
            return g.MeasureString(input, eventFont, int.MaxValue, StringFormat.GenericTypographic).Width;
        }
    }
}
