using System;
using System.Collections.Generic;
using UnityEngine;

namespace Sun {
    [Serializable]
    internal class SerializableDictionary<TKey, TValue> {
        [SerializeField] internal List<TKey> keys = new List<TKey>();
        [SerializeField] internal List<TValue> values = new List<TValue>();

        internal TValue this[TKey key] {
            set {
                for (int i = 0; i < keys.Count; i++) {
                    if (keys[i].Equals(key)) {
                        values[i] = value;
                        return;
                    }
                }

                keys.Add(key);
                values.Add(value);
            }
        }

        internal void Remove(TKey key) {
            for (int i = 0; i < keys.Count; i++) {
                if (keys[i].Equals(key)) {
                    keys.RemoveAt(i);
                    values.RemoveAt(i);
                    return;
                }
            }
        }

        [Serializable]
        internal class SerializableKeyValuePair {
            public TKey key;
            public TValue value;
        }

        public void RemoveAt(int index) {
            keys.RemoveAt(index);
            values.RemoveAt(index);
        }
    }
}