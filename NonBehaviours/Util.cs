using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants {

        private static Vector3 _NullVector = new Vector3(0f, float.PositiveInfinity, 0f);

        //Don't do this, this is dirty. But this is sort of what we want, 
        public static List<GameObject> PlayerCharacterList = new List<GameObject>();
        public static List<GameObject> GlobalEnemyCharacterList = new List<GameObject>();

        public static Vector3 NullVector {
            get {
                return _NullVector;
            }
        }

    }

namespace Util
{
    public class Objects {
        public static Object find<T>(string name) {

            UnityEngine.Object[] foundAssets = Resources.FindObjectsOfTypeAll(typeof(T));

            foreach (var asset in foundAssets)
            {
                if(string.Compare(asset.name, name) == 0) {
                    return asset;
                }
            }
            
            Debug.LogWarning("Util.Object.find<T> : Could not find any matching assets of type " + typeof(T) + " with name " + name);

            return null;

        }   
    }

    public class Timer {
        private float start;
        private bool locked;
        public bool Locked {
            get {
                return locked;
            }
        }
        public float Start {
            get {
                return start;
            }
        }
        private float cur;
        public float Cur {
            get {
                return Mathf.Max(cur, 0f);
            }
        }
        public bool Done {
            get {
                return cur <= 0;
            }
        }
        public Timer(float startTime = 1f) {
            locked = false;
            start = startTime;
            cur = start;
        }
        public void Lock() {
            locked = true;
        }
        public void Unlock() {
            locked = false;
        }
        public void Set(float t) {
            start = t;
            cur = start;
        }

        public void ResetTime() {
            Set(start);
        }

        public void Tick(float t) {
            if(!locked)
                cur -= t;
        }

        public bool CheckAndTick(float t) {
            bool done = Done;
            Tick(t);
            //if(done) ResetTime();

            return done;
        }
    }
    
    public class Charger {
        private float end;
        private bool locked;
        public bool Locked {
            get {
                return locked;
            }
        }
        public float End {
            get {
                return end;
            }
        }
        private float cur;
        public float Cur {
            get {
                return Mathf.Min(cur, end);
            }
        }
        public bool Done {
            get {
                return cur >= end;
            }
        }

        public Charger(float endTime = 1f) {
            locked = false;
            end = endTime;
            cur = 0f;
        }
        public void Lock() {
            locked = true;
        }
        public void Unlock() {
            locked = false;
        }
        public void Set(float t) {
            end = t;
        }

        public void ResetTime() {
            cur = 0f;
        }

        public void Tick(float t) {
            if(!locked)
                cur += t;
        }

        public bool CheckAndTick(float t) {
            bool done = Done;
            Tick(t);
            //if(done) ResetTime();

            return done;
        }
    }
}