using Electric.AUProximity.Hub;
using Electric.AUProximity.Models;
using Impostor.Api.Events;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using Microsoft.AspNetCore.SignalR;

namespace Electric.AUProximity
{
    public class AUProximityListener : IEventListener
    {
        private readonly IHubContext<ProximityHub, IProximityHub> _proximityHub;

        public AUProximityListener(IHubContext<ProximityHub, IProximityHub> proximityHub)
        {
            this._proximityHub = proximityHub;
        }

        [EventListener]
        public void GameStart(IGameStartedEvent e)
        {
            _proximityHub.Clients.Group(e.Game.Code).GameStarted();
            _proximityHub.Clients.Group(e.Game.Code).MapChange(e.Game.Options.Map);
        }

        [EventListener]
        public void GameHostChangedEvent(IGameHostChangedEvent e)
        {
            if (e.NewHost == null)
            {
                return; // No new host, game ending
            }
            _proximityHub.Clients.Group(e.Game.Code).HostChange(e.NewHost.Client.Name);
        }

        [EventListener]
        public void OnPlayerMove(IPlayerMovementEvent e)
        {
            var position = e.PlayerControl.NetworkTransform.Position;
            _proximityHub.Clients.Group(e.Game.Code).PlayerMove(
                e.PlayerControl.PlayerInfo.PlayerName,
                new Pose(position.X, position.Y));
        }
        [EventListener]
        public void MeetingCalled(IMeetingStartedEvent e)
        {
            _proximityHub.Clients.Group(e.Game.Code).MeetingCalled();
        }
        [EventListener]
        public void PlayerMurdered(IPlayerMurderEvent e)
        {
            _proximityHub.Clients.Group(e.Game.Code).PlayerExiled(e.Victim.PlayerInfo.PlayerName);
        }

        [EventListener]
        public void PlayerExiled(IPlayerExileEvent e)
        {
            // Called when MeetingHud RPC VotingComplete has a player to be voted out.
            _proximityHub.Clients.Group(e.Game.Code).PlayerExiled(e.PlayerControl.PlayerInfo.PlayerName);
        }

        // This event requires a custom fork of Impostor
#if FORK
        [EventListener]
        public void RepairSystem(IPlayerRepairSystemEvent e)
        {
            if (e.SystemType == SystemTypes.Sabotage && (SystemTypes)e.Amount == SystemTypes.Comms)
            {
                // Skeld, MiraHQ, or Polus Comms has been sabotaged
                _proximityHub.Clients.Group(e.Game.Code).CommsSabotage(false);
            }
            if (e.SystemType == SystemTypes.Comms)
            {
                if (e.Amount == 1)
                {
                    _proximityHub.Clients.Group(e.Game.Code).CommsSabotage(false);

                }
                else
                {
                    _proximityHub.Clients.Group(e.Game.Code).CommsSabotage(true);
                }
            }
        }
#endif

        [EventListener]
        public void GameEnd(IGameEndedEvent e)
        {
            _proximityHub.Clients.Group(e.Game.Code).GameEnd();
        }
    }
}
