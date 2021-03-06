﻿using System;
using System.Collections.Generic;
using System.Linq;
using MyTunes.Common.ViewModels;
using MyTunes.Dominio;
using MyTunes.Repository;

namespace MyTunes.Services.Entities
{
    public class PlayListService : IDisposable
    {
        private IRepository<Playlist> _playListRepository;
        private IRepository<Customer> _customerRepository;
        private IRepository<Track> _trackRepository;

        public PlayListService(IRepository<Playlist> playListsRepository, IRepository<Customer> customerRepository, IRepository<Track> trackRepository)
        {
            this._playListRepository = playListsRepository;
            this._customerRepository = customerRepository;
            this._trackRepository = trackRepository;
        }
        public IEnumerable<PlayListViewModel> GetPlayLists(string userName)
        {
            var customer = _customerRepository.Get().FirstOrDefault(c=>c.Email==userName);
            if (customer == null) throw new InvalidOperationException(string.Format("Cliente no encontrado {0}", userName));
            var playLists = _playListRepository.Get().Where(x=>x.CustomerId==customer.Id).ToList(); // PlayList
            // aqui se tiene que hacer un mapeo del dominio al viewmodel
            return playLists.Select(playList => new PlayListViewModel(playList)).ToList();
        }
         
        public void Dispose()
        {
            _playListRepository = null;
        }

        public void Create(PlaylistCreateViewModel model)
        {
            var customer = _customerRepository.Get().FirstOrDefault(c => c.Email == model.CustomerUserName);
            if (customer == null) throw new InvalidOperationException(string.Format("Cliente no encontrado {0}", model.CustomerUserName));
            var playList = new Playlist(){Name = model.Nombre,CustomerId = customer.Id};
            _playListRepository.Create(playList);
        }

        public PlaylistEditViewModel Get(int playlistId)
        {
            var playList = _playListRepository.Get().FirstOrDefault(x => x.Id == playlistId);
            if (playList== null) throw new InvalidOperationException("Playlist no encontrado");
            return new PlaylistEditViewModel(){ Name = playList.Name, Id = playList.Id, CustomerId=playList.CustomerId, Tracks = playList.Track.Select(track => new TracksListViewModel(track,playList.Id)).ToList() };
        }

        public IEnumerable<TracksListViewModel> GetTracksFrom(PlaylistSearchTrackViewModel request)
        {
            var tracklist = _trackRepository.Get().Where(x=>x.Name.Contains(request.TrackName)).ToList();
            return tracklist.Select(track => new TracksListViewModel(track, request.PlayListId)).ToList();
        }

        public void AddTrack(int playListId, int trackId)
        {
            var playList = _playListRepository.Get().FirstOrDefault(x => x.Id == playListId);
            if (playList == null) throw new InvalidOperationException("Playlist no encontrado");
            var track = _trackRepository.Get().FirstOrDefault(x => x.Id==trackId);
            if (playList == null) throw new InvalidOperationException("Track no encontrado");

            playList.Track.Add(track);
            _playListRepository.Update(playList);

        }
    }
}
