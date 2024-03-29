﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Mobile;
using Wellness.Mobile.AsyncHelper;
using Wellness.Model;
using Wellness.Model.Requests;
using Xamarin.Forms;

namespace Mobile.ViewModels
{
    class TreningViewModel: BaseViewModel
    {
        private readonly APIService _apiService;
        private readonly APIService _apiService_TipTreninga;
        private readonly APIService _apiService_ClanPrisustvo;
        private readonly Wellness.Model.Requests.ClanViewRequest _clan = null;
        private readonly bool _historyOnly=false;

        public ObservableCollection<Mobile.Models.TreningModel> Trening { get; set; }
        public ObservableCollection<Wellness.Model.TipTreninga> TipTreninga { get; set; }
        public ObservableCollection<string> SearchFilter { get; set; }


        public TreningViewModel(Wellness.Model.Requests.ClanViewRequest clan, bool historyOnly = false)
        {
            _apiService = new APIService("trening");
            _apiService_TipTreninga = new APIService("TipTreninga");
            _apiService_ClanPrisustvo = new APIService("ClanPrisustvo");
            _clan = clan;
            _historyOnly = historyOnly;
            //--------------------------------------------------------
            Trening = new ObservableCollection<Mobile.Models.TreningModel>();
            TipTreninga = new ObservableCollection<Wellness.Model.TipTreninga>();
            SearchFilter = new ObservableCollection<string>();

            InitCommand = new Command(async () => await Init());

            
            SearchFilter.Add("Svi");
            SearchFilter.Add("Prijavljeni");
            SearchFilter.Add("Ne prijavljeni");
            _selectedSearchFilter = SearchFilter[0];
            var SviTipTreninga = new TipTreninga()
            {
                Id = 0,
                TipTreninga1 = "Svi"
            };
            TipTreninga.Add(SviTipTreninga);

            var listTipTreninga = AsyncHelpers.RunSync<List<Wellness.Model.TipTreninga>>(() => _apiService_TipTreninga.Get<List<Wellness.Model.TipTreninga>>(null));
            foreach (Wellness.Model.TipTreninga x in listTipTreninga)
            {
                TipTreninga.Add(x);
            }
            _selectedTipTreninga = TipTreninga[0];


        }

        public TreningViewModel()
        {

        }

        #region valued
        Wellness.Model.TipTreninga _selectedTipTreninga;
        public Wellness.Model.TipTreninga SelectedTipTreninga
        {

            get { return _selectedTipTreninga; }
            set
            {
                SetProperty(ref _selectedTipTreninga, value);
                if (value != null)
                {
                    InitCommand.Execute(null);
                }
            }
        }

        string _selectedSearchFilter = null;
        public string selectedSearchFilter
        {

            get { return _selectedSearchFilter; }
            set
            {

                _selectedSearchFilter = value;
                if (value != null)
                {
                    InitCommand.Execute(null);
                }

            }
        }

        #endregion valued;

        public ICommand InitCommand { get; set; }
        public async Task Init()
        {
            int ClanId = _clan.Id;


            var search = new TreningSearchRequest()
            {
                DatumTreninga = DateTime.MinValue,
                TipTreningaID = 0
            };

            search.TipTreningaID = SelectedTipTreninga.Id;


            var list = await _apiService.Get<List<Wellness.Model.Trening>>(search);


            //filter if historyOnly = true
            //u toDeleteList se nalaze treninga koji nisu odrzani
            var newList = new List<Wellness.Model.Trening>();
            if (_historyOnly == true)
            {

                //uklanjamo treninge koji se nisu odrzali
                for (int i = 0;i<list.Count;i++)
                {

                    if (list[i].Odrzan == null) list[i].Odrzan = false;
                    Wellness.Model.Trening x = list[i];
                    
                    if (x.Odrzan == true)
                    {
                        //clan je prisustvovo treninug x
                        var prisustvoList = AsyncHelpers.RunSync<List<Wellness.Model.ClanPrisustvo>>(() => _apiService_ClanPrisustvo.Get<List<Wellness.Model.ClanPrisustvo>>(new ClanPrisustvoSearchRequest() { TreningId = x.Id, ClanId = ClanId }));
                        //ako _clan nije prosustvovo treningu x izbacujemo x iz liste
                        if (prisustvoList.Count > 0)
                        { 
                            newList.Add(list[i]);
                        }
                    }
                }


                list = newList;
            }
            else
            {
                newList = list.Where(p => p.Odrzan == false || p.Odrzan == null).ToList();
                list = newList;

                foreach (var x in list)
                {
                    if (x.Odrzan == null)
                        x.Odrzan = false;
                }
            }
  
            //-----------------------------------------------------------------------------------------------
            Trening.Clear();
            for (int i = 0; i < list.Count; i++)
            {

                Wellness.Model.Trening x = list[i];

                //var trenutnoPrisutnih =await _apiService_ClanPrisustvo.Get<List<Model.ClanPrisustvo>>();
                var trenutnoPrisutnih = AsyncHelpers.RunSync<List<Wellness.Model.ClanPrisustvo>>(() => _apiService_ClanPrisustvo.Get<List<Wellness.Model.ClanPrisustvo>>(new ClanPrisustvoSearchRequest() { TreningId = x.Id }));

                var treningModel = new Mobile.Models.TreningModel()
                {

                    Id = x.Id,
                    DatumT=x.DatumTreninga,
                    DatumTreninga = x.DatumTreninga.ToString("dd.MM.yyyy"),
                    pocetak = x.VrijemePocetak,
                    kraj = x.VrijemeKraj,
                    MaxPrisutnih = x.MaksimalnoPrisutnih,
                    Trener = x.Trener.Radnik.Osoba.Ime + " " + x.Trener.Radnik.Osoba.Prezime,
                    TipTreninga = x.TipTreninga.TipTreninga1,
                    ClanId = ClanId,
                    TrenutnoPrisutnih = trenutnoPrisutnih.Count,
                    Odrzan = x.Odrzan,
                    Image = x.TipTreninga.Image

                };

                var prisustvo = AsyncHelpers.RunSync<List<Wellness.Model.ClanPrisustvo>>(() => _apiService_ClanPrisustvo.Get<List<Wellness.Model.ClanPrisustvo>>(new ClanPrisustvoSearchRequest() { TreningId = x.Id, ClanId = ClanId }));

                if (prisustvo.Count > 0 && (selectedSearchFilter == "Svi" || selectedSearchFilter == "Prijavljeni"))
                {
                    treningModel.ClanPrisustvovaoTreningu = prisustvo[0].Prisustvovao;
                    treningModel._Prisustvuje = true;
                    treningModel.PrisustvoId = prisustvo[0].Id;
                    if (prisustvo[0].Ocjena != null)
                    {
                        treningModel._Ocjena = (int)prisustvo[0].Ocjena;
                        treningModel.Ocjenjen = true;
                    }
                    else
                    {
                        treningModel._Ocjena = 0;
                        treningModel.Ocjenjen = false;
                    }
                    Trening.Add(treningModel);
                }
                else
                {
                    if (prisustvo.Count == 0 && (selectedSearchFilter == "Svi" || selectedSearchFilter == "Ne prijavljeni"))
                    {
                        treningModel.ClanPrisustvovaoTreningu = false;
                        treningModel._Prisustvuje = false;
                        treningModel.Ocjenjen = false;
                        treningModel.PrisustvoId = 0;
                        treningModel._Ocjena = 0;
                        Trening.Add(treningModel);
                    }
                }



            }

        }
    }
}
