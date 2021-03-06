﻿using System;

namespace NSubstitute.Core
{
    internal class ReturnObservable<T> : IObservable<T>
    {
        T _value;

        public ReturnObservable() : this(default(T)) { }

        public ReturnObservable(T value)
        {
            _value = value;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(_value);
            observer.OnCompleted();

            return EmptyDisposable.Instance;
        }
    }

    internal class EmptyDisposable : IDisposable
    {
        static IDisposable _empty = new EmptyDisposable();
        public static IDisposable Instance
        {
            get
            {
                return _empty;
            }
        }

        public void Dispose() { }
    }
}