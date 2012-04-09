using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Hanasu.Core
{
    //http://msdn.microsoft.com/en-us/library/dd990377.aspx
    public class ObservableQueue<T> : Queue<T>, IObservable<T>, IDisposable, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private List<IObserver<T>> Observers { get; set; }
        public ObservableQueue()
        {
            Observers = new List<IObserver<T>>();
        }
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (!Observers.Contains(observer))
                Observers.Add(observer);
            return new ObservableQueueUnsubscriber<T>(Observers, observer);
        }
        private class ObservableQueueUnsubscriber<T> : IDisposable
        {
            private IObserver<T> observer;
            private List<IObserver<T>> observers;
            public ObservableQueueUnsubscriber(List<IObserver<T>> _obsers, IObserver<T> ob)
            {
                observers = _obsers;
                observer = ob;
            }
            public void Dispose()
            {
                if (observer != null && observers.Contains(observer))
                    observers.Remove(observer);
            }
        }
        public bool IsEmpty { get { return base.Count == 0; } }
        public new void Enqueue(T obj)
        {
            base.Enqueue(obj);

            //Probably shouldn't be here.
            Application.Current.Dispatcher.Invoke(
                new EmptyDelegate(() =>
                {
                    HandleCollectionChanged(NotifyCollectionChangedAction.Add, obj,base.Count-1);
                    HandlePropertyChanged();
                }));
        }
        public new T Dequeue()
        {
            var itm = base.Dequeue();
            foreach (var obs in Observers)
                if (itm != null)
                    obs.OnNext(itm);
                else
                    obs.OnError(new Exception());

            //Probably shouldn't be here.
            Application.Current.Dispatcher.Invoke(new EmptyDelegate(
                () =>
                {
                    HandleCollectionChanged(NotifyCollectionChangedAction.Remove, itm, 0);
                    HandlePropertyChanged();
                }));

            return itm;
        }

        public void Dispose()
        {
            base.Clear();
            foreach (var ob in Observers)
                ob.OnCompleted();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void HandleCollectionChanged(NotifyCollectionChangedAction e, Object itm, int ind)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(e, itm, ind));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void HandlePropertyChanged()
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(""));
        }
        private delegate void EmptyDelegate();
    }
}
