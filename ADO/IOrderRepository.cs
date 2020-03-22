using System;
using System.Collections.Generic;
using ADO.Models;

namespace ADO
{
    public interface IOrderRepository : IRepository<Order>
    {
        IEnumerable<Order> Get();
        
        Order GetById(int id);

        void Create(Order newOrder);

        void Update(Order order);

        void Delete(int id);
    }


    //public class SQLBookRepository : IRepository<Book>
    //{
    //    private BookContext db;

    //    public SQLBookRepository()
    //    {
    //        this.db = new BookContext();
    //    }

    //    public IEnumerable<Book> GetBookList()
    //    {
    //        return db.Books;
    //    }

    //    public Book GetBook(int id)
    //    {
    //        return db.Books.Find(id);
    //    }

    //    public void Create(Book book)
    //    {
    //        db.Books.Add(book);
    //    }

    //    public void Update(Book book)
    //    {
    //        db.Entry(book).State = EntityState.Modified;
    //    }

    //    public void Delete(int id)
    //    {
    //        Book book = db.Books.Find(id);
    //        if (book != null)
    //            db.Books.Remove(book);
    //    }

    //    public void Save()
    //    {
    //        db.SaveChanges();
    //    }

    //    private bool disposed = false;

    //    public virtual void Dispose(bool disposing)
    //    {
    //        if (!this.disposed)
    //        {
    //            if (disposing)
    //            {
    //                db.Dispose();
    //            }
    //        }
    //        this.disposed = true;
    //    }

    //    public void Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }
    //}
}
