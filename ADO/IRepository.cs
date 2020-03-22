using System;
using System.Collections.Generic;

namespace ADO
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> Get(); 
        T GetById(int id); 
        void Create(T item); 
        void Update(T item); 
        void Delete(int id);
    }
}