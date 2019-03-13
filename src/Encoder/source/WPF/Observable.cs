using System;
using System.Collections.Generic;
using System.ComponentModel;

// MIT License - Ben Siron

// Knockout.JS style observables for .NET, implementing INotifyPropertyChanged - useful for WPF data-binding
// See unit tests for example uses and verification of correctness.
// Note: unlike Javascript, .NET is multithreaded, and so whereas multithreading can cause no problems with javascript knockout,
//  in .NET there is potential trouble.
// Computed functions MUST run synchronously in a single thread.
// Any dependencies to observables encountered due to a computed function running code in other threads or in separately re-entrant code
//  (such as by calling Application.DoEvents) may be ignored or applied to the wrong computed functions.

namespace UGTS.Encoder.WPF
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
        /// all computed observables which depend on this observable
        /// </summary>
        private readonly List<ObservableBase> _dependents = new List<ObservableBase>();

        /// <summary>
        /// reference-count applied to all computed observables - a positive number means that the computed is currently being watched.
        /// </summary>
        [ThreadStatic]
        private static Dictionary<ObservableBase, int> _watchingOnThisThread;

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
            if (_watchingOnThisThread == null)
            {
                _watchingOnThisThread = new Dictionary<ObservableBase, int>();
            }

            if (!_watchingOnThisThread.ContainsKey(computed))
            {
                _watchingOnThisThread[computed] = 0;
            }

            _watchingOnThisThread[computed] += start ? 1 : -1;
            if (!start && (_watchingOnThisThread[computed] <= 0))
                _watchingOnThisThread.Remove(computed);
        }

        protected static IEnumerable<ObservableBase> WatchingOnThisThread
        {
            get
            {
                if (_watchingOnThisThread == null) _watchingOnThisThread = new Dictionary<ObservableBase, int>();
                return _watchingOnThisThread.Keys;
            }
        }

        protected void AddDependents()
        {
            // if any computed objects are currently being watched and have not thus far been noted as dependent, mark them now.
            foreach (var dependant in WatchingOnThisThread)
            {
                if (!_dependents.Contains(dependant))
                    _dependents.Add(dependant);
            }
        }

        protected void UpdateDependents(object changes)
        {
            foreach (var d in _dependents)
            {
                d.OnValueChanged(changes);
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

        protected T InternalValue, OldInternalValue;

        public Observable()
        {
            Value = default(T);
            OldInternalValue = InternalValue;
        }

        public Observable(T initialValue)
        {
            Value = initialValue;
            OldInternalValue = InternalValue;
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
            return EqualityComparer?.Invoke(InternalValue, value) ?? Object.Equals(InternalValue, value);
        }

        public virtual T Value
        {
            get
            {
                AddDependents();
                return InternalValue;
            }
            set
            {
                if (IsEqual(value)) return;
                OldInternalValue = InternalValue;
                InternalValue = value;
                var changes = new ValueChangedEventArgs<T>(OldInternalValue, InternalValue);
                OnValueChanged(changes);
                UpdateDependents(changes);
            }
        }

        protected override void OnValueChanged(object changes)
        {
            ValueChanged?.Invoke(this, (ValueChangedEventArgs<T>)changes);
            OnPropertyChanged("Value");
        }
    }

    public class Computed<T> : Observable<T>
    {
        private readonly Func<T> _computed;

        public Computed(Func<T> func)
        {
            _computed = func;
            InternalValue = Value; // this serves to initialize dependencies
        }

        public override T Value
        {
            get
            {
                InternalValue = default(T);
                try
                {
                    if (_computed != null)
                    {
                        StartWatching(this);
                        InternalValue = _computed();
                    }
                }
                finally
                {
                    if (_computed != null)
                        StopWatching(this);
                }

                OldInternalValue = InternalValue;
                return InternalValue;
            }
        }

        protected override void OnValueChanged(object changes)
        {
            var before = OldInternalValue;
            var computedChanges = new ValueChangedEventArgs<T>(before, Value);
            base.OnValueChanged(computedChanges);
        }
    }
}
