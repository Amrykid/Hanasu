using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services
{
    public interface IService
    {
        
    }
    public interface IStaticService : IService
    {
    }
    public interface INonStaticService : IService
    {
        void Initialize();
    }
    public abstract class BaseService<T>: INonStaticService
    {
        public bool IsInitialized { get; protected set; }
        static BaseService()
        {

        }
        public abstract void Initialize();
        public T Instance { get; protected set; }
    }
}
