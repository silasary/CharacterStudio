using System;
using System.Linq;
using System.Text;

namespace ParagonLib
{
    public class D20Currency : IComparable
    {
        public static D20Currency Zero = new D20Currency();
        private double ad;
        private double cp;
        private double gp;
        private double pp;
        private double sp;

        public D20Currency()
        {
            this.cp = (this.sp = (this.gp = (this.pp = (this.ad = 0.0))));
        }

        public D20Currency(D20Currency from)
        {
            this.Copy(from);
        }

        public D20Currency(double copper, double silver, double gold, double platinum, double astral)
        {
            this.cp = copper;
            this.sp = silver;
            this.gp = gold;
            this.pp = platinum;
            this.ad = astral;
        }

        public D20Currency(RulesElement Rule)
        {
            if (Rule.Specifics.ContainsKey("Copper"))
                this.cp = int.Parse(Rule.Specifics["Copper"].FirstOrDefault());
            if (Rule.Specifics.ContainsKey("Copper"))
                this.sp = int.Parse(Rule.Specifics["Silver"].FirstOrDefault());
            if (Rule.Specifics.ContainsKey("Copper"))
                this.gp = int.Parse(Rule.Specifics["Gold"].FirstOrDefault());
            this.Normalize();
        }

        public double Astral
        {
            get
            {
                return this.ad;
            }
            set
            {
                this.ad = value;
            }
        }

        public double Copper
        {
            get
            {
                return this.cp;
            }
            set
            {
                this.cp = value;
            }
        }

        public double EquivalentInCopper
        {
            get
            {
                return this.Astral * 1000000.0 + this.Platinum * 10000.0 + this.Gold * 100.0 + this.Silver * 10.0 + this.Copper;
            }
        }

        public double EquivalentInGold
        {
            get
            {
                return this.EquivalentInCopper / 100.0;
            }
        }

        public double Gold
        {
            get
            {
                return this.gp;
            }
            set
            {
                this.gp = value;
            }
        }

        public bool IsNegative
        {
            get
            {
                return this.EquivalentInGold < 0.0;
            }
        }

        public double Platinum
        {
            get
            {
                return this.pp;
            }
            set
            {
                this.pp = value;
            }
        }

        public string ShortDisplayValue
        {
            get
            {
                return string.Format("{0} gp", this.EquivalentInGold);
            }
        }

        public double Silver
        {
            get
            {
                return this.sp;
            }
            set
            {
                this.sp = value;
            }
        }

        public bool Add(D20Currency value)
        {
            this.cp += value.cp;
            this.sp += value.sp;
            this.gp += value.gp;
            this.pp += value.pp;
            this.ad += value.ad;
            return true;
        }

        public bool CheckAnyNegative()
        {
            return this.cp < 0.0 || this.sp < 0.0 || this.gp < 0.0 || this.pp < 0.0;
        }

        public int CompareTo(D20Currency other)
        {
            if (other == null)
            {
                other = new D20Currency();
            }
            return this.EquivalentInGold.CompareTo(other.EquivalentInGold);
        }

        public void Copy(D20Currency value)
        {
            this.cp = value.cp;
            this.sp = value.sp;
            this.gp = value.gp;
            this.pp = value.pp;
            this.ad = value.ad;
        }

        public bool FromString(string str)
        {
            this.cp = (this.sp = (this.gp = (this.pp = (this.ad = 0.0))));
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }
            while (!string.IsNullOrEmpty(str))
            {
                int split = str.IndexOf(';');
                if (split == -1) split = str.IndexOf(',');
                string part;
                if (split != -1)
                {
                    part = str.Substring(0, split);
                    str = str.Substring(split + 1);
                }
                else
                {
                    part = str;
                    str = "";
                }
                if (!this.FromPartialString(part))
                {
                    return false;
                }
            }
            return true;
        }

        int IComparable.CompareTo(object _other)
        {
            D20Currency d20Currency = _other as D20Currency;
            if (d20Currency == null)
            {
                return -1;
            }
            return this.EquivalentInGold.CompareTo(d20Currency.EquivalentInGold);
        }

        public void Multiply(int multiplier)
        {
            this.cp *= (double)multiplier;
            this.sp *= (double)multiplier;
            this.gp *= (double)multiplier;
            this.pp *= (double)multiplier;
            this.ad *= (double)multiplier;
        }

        public void Normalize()
        {
            this.ExchangeCoins(ref this.cp, ref this.sp, 10.0);
            this.ExchangeCoins(ref this.sp, ref this.gp, 10.0);
            this.ExchangeCoins(ref this.gp, ref this.pp, 100.0);
            this.ExchangeCoins(ref this.pp, ref this.ad, 100.0);
        }

        public void ReadFromArray(double[] values)
        {
            this.Copper = values[0];
            this.Silver = values[1];
            this.Gold = values[2];
            this.Platinum = values[3];
            this.Astral = values[4];
        }

        public bool Subtract(D20Currency value)
        {
            this.cp -= value.cp;
            this.sp -= value.sp;
            this.gp -= value.gp;
            this.pp -= value.pp;
            this.ad -= value.ad;
            return true;
        }

        public double[] ToArray()
        {
            return new double[]
			{
				this.Copper,
				this.Silver,
				this.Gold,
				this.Platinum,
				this.Astral
			};
        }

        public void ToGold()
        {
            this.gp = this.EquivalentInGold;
            this.cp = (this.sp = (this.pp = (this.ad = 0.0)));
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (this.ad != 0.0)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append("; ");
                }
                stringBuilder.AppendFormat("{0:#,0} ad", new object[]
				{
					this.ad
				});
            }
            if (this.pp != 0.0)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append("; ");
                }
                stringBuilder.AppendFormat("{0:#,0} pp", new object[]
				{
					this.pp
				});
            }
            if (this.gp != 0.0)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append("; ");
                }
                stringBuilder.AppendFormat("{0:#,0} gp", new object[]
				{
					this.gp
				});
            }
            if (this.sp != 0.0)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append("; ");
                }
                stringBuilder.AppendFormat("{0:#,0} sp", new object[]
				{
					this.sp
				});
            }
            if (this.cp != 0.0)
            {
                if (stringBuilder.Length != 0)
                {
                    stringBuilder.Append("; ");
                }
                stringBuilder.AppendFormat("{0:#,0} cp", new object[]
				{
					this.cp
				});
            }
            if (stringBuilder.Length == 0)
            {
                stringBuilder.Append("0 gp");
            }
            return stringBuilder.ToString();
        }

        private void ExchangeCoins(ref double smaller, ref double larger, double rate)
        {
            if (smaller > 0.0)
            {
                larger += Math.Floor(smaller / rate);
                smaller %= rate;
                return;
            }
            if (smaller < 0.0)
            {
                double num = Math.Ceiling(smaller / -rate);
                double num2 = num * rate;
                smaller += num2;
                larger -= num;
            }
        }

        private bool FromPartialString(string part)
        {
            part = part.Trim();
            int space = part.IndexOf(' ');
            if (space == -1)
            {
                return false;
            }
            double count;
            if (!double.TryParse(part.Substring(0, space), out count))
            {
                return false;
            }
            part = part.Substring(space + 1).ToLower();
            switch (part)
            {
                case "cp":
                    this.cp += count;
                    break;

                case "sp":
                    this.sp += count;
                    break;

                case "gp":
                    this.gp += count;
                    break;

                case "pp":
                    this.pp += count;
                    break;

                case "ad":
                    this.ad += count;
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}