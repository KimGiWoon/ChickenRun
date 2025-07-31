using Photon.Pun;
using UnityEngine;

namespace Kst
{
    public class GameManager_Map3 : MonoBehaviourPun
    {
        public static GameManager_Map3 Instance;

        void Awake()
        {
            Instance = this;
        }

        #region RPC

        [PunRPC]
        public void AddScore(int actorNumber, int score)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                ScoreManager.Instance.AddScroe(score);
            }
        }

        [PunRPC]
        public void MinusScore(int actorNumber, int score)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                ScoreManager.Instance.AddScroe(score);
            }
        }

        [PunRPC]
        public void GiveEgg(int actorNumber, int eggAmount)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == actorNumber)
            {
                EggManager.Instance.GainEgg(eggAmount);
            }
        }
        #endregion
    }
}
