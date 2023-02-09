using OSCore.System.Interfaces.Pooling;
using UnityEngine;

namespace OSCore.System.Pooling {
    public class SlidingPool : IPool {
        private readonly GameObject[] pool;
        private int nextInst;

        public SlidingPool(GameObject prefab, int size = 5) {
            nextInst = 0;
            pool = new GameObject[size];
            for (int i = 0; i < size; i++) pool[i] = GameObject.Instantiate(prefab);
        }

        public GameObject Instantiate(Vector3 position, Quaternion rotation) {
            GameObject next = pool[nextInst];
            next.transform.SetPositionAndRotation(position, rotation);

            IPooled pooled = next.GetComponent<IPooled>();
            if (pooled is not null) pooled.Go();
            IncNext();

            return next;
        }

        private void IncNext() {
            nextInst++;
            if (nextInst >= pool.Length) nextInst = 0;
        }
    }
}
