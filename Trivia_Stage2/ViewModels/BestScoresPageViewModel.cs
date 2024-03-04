﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Trivia_Stage2.Models;
using Trivia_Stage2.Services;
namespace Trivia_Stage2.ViewModels
{
    public class BestScoresPageViewModel : ViewModel
    {
        private Service service;
        public bool IsReloading;
        public ICommand Reload { get; set; }
        public ICommand Search { get; set; }
        public string SearchBar;
        public bool IsOrdered;
        public Player SelectedPlayer { get { return selectedPlayer; } set { count = value; OnPropertyChanged(); } }
        private Player selectedPlayer;
        public ObservableCollection<Player> players { get; private set; }

        public BestScoresPageViewModel(Service service_)
        {
            IsOrdered = false;
            service = service_;
            players = new ObservableCollection<Player>(service.Players);
            IsReloading = false;
            SelectedPlayer=new Player();
            Reload=new Command(async () => await TaskReload());
            Search = new Command(async () => await TaskSearch());
        }
        public async Task TaskReload()
        {
            if (IsReloading) return;
            IsReloading=true;
            players.Clear();
            foreach (Player plr in service.Players)
                players.add(plr);
            if (IsOrdered)
            {
                players = new ObservableCollection<Player>(players.OrderByDescending(Player p => p.Points));
                IsOrdered = false;
            }
            else
            {
                players = new ObservableCollection<Player>(players.OrderByAscending(Player p => p.Points));
                IsOrdered = true;
            }
            IsReloading = false;

        }
        public async Task TaskSearch()
        {
            if (int.TryParse(SearchBar, out int search))
            {
                players = new ObservableCollection<Player>(players.Where(Player player => player.Points == search));
            }
        }
    }
}
