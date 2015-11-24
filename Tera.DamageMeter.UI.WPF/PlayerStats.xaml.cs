﻿using System;
using System.Collections.Concurrent;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Tera.DamageMeter.UI.WPF
{
    /// <summary>
    ///     Logique d'interaction pour PlayerStats.xaml
    /// </summary>
    public partial class PlayerStats
    {
        private Skills _windowSkill;
        public ImageSource Image;

        public PlayerStats(PlayerInfo playerInfo)
        {
            InitializeComponent();
            PlayerInfo = playerInfo;
            Image = ClassIcons.Instance.GetImage(PlayerInfo.Class).Source;
            Class.Source = Image;
            LabelName.Content = PlayerName;
        }

        public PlayerInfo PlayerInfo { get; set; }

        public string Dps => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Dps) + "/s";


        public string DamagePart => Math.Round(PlayerInfo.Dealt.DamageFraction) + "%";

        public string Damage => FormatHelpers.Instance.FormatValue(PlayerInfo.Dealt.Damage);


        public string DamageReceived => FormatHelpers.Instance.FormatValue(PlayerInfo.Received.Damage);

        public string CritRate => Math.Round(PlayerInfo.Dealt.CritRate) + "%";


        public string PlayerName => PlayerInfo.Name;

        public void Repaint()
        {
            DpsIndicator.Width = ActualWidth*(PlayerInfo.Dealt.DamageFraction/100);
            LabelDps.Content = Dps;
            LabelDamage.Content = Damage;
            LabelCritRate.Content = CritRate;
            LabelDamagePart.Content = DamagePart;
            LabelDamageReceived.Content = DamageReceived;


            _windowSkill?.Update(Skills());
            
        }

        private ConcurrentDictionary<DamageMeter.Skill, SkillStats> Skills()
        {

            if (UiModel.Instance.Encounter == null)
            {
                return PlayerInfo.Dealt.AllSkills;
            }
            if (PlayerInfo.Dealt.EntitiesStats.ContainsKey(UiModel.Instance.Encounter))
            {
                return PlayerInfo.Dealt.EntitiesStats[UiModel.Instance.Encounter].Skills;
            }
           
            return new ConcurrentDictionary<DamageMeter.Skill, SkillStats>();
            
        }

        private void ShowSkills(object sender, MouseButtonEventArgs e)
        {
            if (_windowSkill == null)
            {
                _windowSkill = new Skills(Skills(), this)
                {
                    Title = PlayerName,
                    CloseMeter = {Content = PlayerInfo.Class + " " + PlayerName + ": CLOSE"}
                };
            }

            _windowSkill.Show();
            _windowSkill.Update(Skills());
        }

        public void CloseSkills()
        {
            _windowSkill?.Close();
            _windowSkill = null;
        }


        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
            var w = Window.GetWindow(this);
            try
            {
                w?.DragMove();
            }
            catch
            {
                Console.WriteLine(@"Exception move");
            }
        }
    }
}