using UnityEngine;

namespace Kst
{
    public class PhotonPooledObject : MonoBehaviour
    {
        public PhotonObjectPool ObjPool { get; private set; }

        public void PooledInit(PhotonObjectPool objPool)
        {
            ObjPool = objPool;
        }

        public void ReturnPool()
        {
            ObjPool.PushPool(this);
        }
    }
}