using General;
using UnityEngine;

namespace Items.Guns
{
    public class Magazine
    {
        public int CurrentAmmo { get; private set; }

        private int Capacity { get; }

        public Magazine(int capacity)
        {
            Capacity = capacity;
            CurrentAmmo = capacity;
        }

        public bool IsEmpty => CurrentAmmo <= 0;
        public bool IsFull => CurrentAmmo >= Capacity;

        public void SubtractAmmo(int amount)
        {
            CurrentAmmo = Mathf.Max(0, CurrentAmmo - amount);
        }

        public void SubtractOne()
        {
            SubtractAmmo(1);
            VandullLogger.Log(CurrentAmmo);
        }
    }
}