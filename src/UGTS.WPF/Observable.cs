using System;
using System.Collections.Generic;
using System.ComponentModel;
using UGTS.Dictionaries;

// MIT License - Ben Siron

// Knockout.JS style observables for .NET, implementing INotifyPropertyChanged - useful for WPF data-binding
// See unit tests for example uses and verification of correctness.
// Note: unlike Javascript, .NET is multithreaded, and so whereas multithreading can cause no problems with javascript knockout,
//  in .NET there is potential trouble.
// Computed functions MUST run synchronously in a single thread.
// Any dependencies to observables encountered due to a computed function running code in other threads or in separately re-entrant code
//  (such as by calling Application.DoEvents) may be ignored or applied to the wrong computed functions.

namespace UGTS.WPF
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public T OldValue, NewValue;

        public ValueChangedEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    public delegate void ValueChangedEventHandler<T>(ObservableBase observable, ValueChangedEventArgs<T> arguments);
    public delegate bool EqualityComparer<T>(T valueA, T valueB);


    public abstract class ObservableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// set of all computed observables which depend on this observable - only the keys are important - the values are null.
        /// </summary>
        private SafeDictionary<ObservableBase, object> myDependents = new SafeDictionary<ObservableBase, object>();

        /// <summary>
        /// reference-count applied to all computed observables - a positive number means that the computed is currently being watched.
        /// </summary>
        [ThreadStatic]
        private static SafeDictionary<ObservableBase, int> theWatchingOnThisThread;

        protected static void StartWatching(ObservableBase computed)
        {
            Watch(computed, true);
        }

        protected static void StopWatching(ObservableBase computed)
        {
            Watch(computed, false);
        }

        private static void Watch(ObservableBase computed, bool start)
        {
            if (theWatchingOnThisThread == null) theWatchingOnThisThread = new SafeDictionary<ObservableBase, int>();
            theWatchingOnThisThread[computed] += start ? 1 : -1;
            if (!start && (theWatchingOnThisThread[computed] <= 0))
                theWatchingOnThisThread.Remove(computed);
        }

        protected static IEnumerable<ObservableBase> WatchingOnThisThread
        {
            get
            {
                if (theWatchingOnThisThread == null) theWatchingOnThisThread = new SafeDictionary<ObservableBase, int>();
                return theWatchingOnThisThread.Keys;
            }
        }

        protected void AddDependents()
        {
            // if any computed objects are currently being watched and have not thus far been noted as dependent, mark them now.
            foreach (var dependant in WatchingOnThisThread)
            {
                if (!myDependents.ContainsKey(dependant))
                    myDependents[dependant] = null;
            }
        }

        protected void UpdateDependents(object changes)
        {
            foreach (var dependant in myDependents.Keys)
            {
                dependant.OnValueChanged(changes);
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected abstract void OnValueChanged(object changes);
    }

    public class Observable<T> : ObservableBase
    {
        public EqualityComparer<T> EqualityComparer;
        public event ValueChangedEventHandler<T> ValueChanged;

        protected T myValue, myOldValue;

        public Observable()
        {
            Value = default(T);
            myOldValue = myValue;
        }

        public Observable(T initialValue)
        {
            Value = initialValue;
            myOldValue = myValue;
        }

        /// <summary>
        /// Allow observables to be implicitly cast to their value.
        /// </summary>
        public static implicit operator T(Observable<T> o)
        {
            return o.Value;
        }

        private bool IsEqual(T value)
        {
            return EqualityComparer?.Invoke(myValue, value) ?? Object.Equals(myValue, value);
        }

        public virtual T Value
        {
            get
            {
                AddDependents();
                return myValue;
            }
            set
            {
                if (IsEqual(value)) return;
                myOldValue = myValue;
                myValue = value;
                var changes = new ValueChangedEventArgs<T>(myOldValue, myValue);
                OnValueChanged(changes);
                UpdateDependents(changes);
            }
        }

        protected override void OnValueChanged(object changes)
        {
            if (ValueChanged != null)
                ValueChanged(this, (ValueChangedEventArgs<T>)changes);

            OnPropertyChanged("Value");
        }
    }

    public class Computed<T> : Observable<T>
    {
        private Func<T> myComputed;

        public Computed(Func<T> func)
        {
            myComputed = func;
            myValue = Value; // this serves to initialize dependencies
        }

        public override T Value
        {
            get
            {
                myValue = default(T);
                try
                {
                    if (myComputed != null)
                    {
                        StartWatching(this);
                        myValue = myComputed();
                    }
                }
                finally
                {
                    if (myComputed != null)
                        StopWatching(this);
                }

                myOldValue = myValue;
                return myValue;
            }
        }

        protected override void OnValueChanged(object changes)
        {
            var before = myOldValue;
            var computedChanges = new ValueChangedEventArgs<T>(before, Value);
            base.OnValueChanged(computedChanges);
        }
    }
}
