namespace Stockpile.DataProvider.Lucandrew
{
    public class ObjectReference
    {
        public string Id { get { return _id; } }
        private readonly string _id;

        protected readonly Database Context;

        internal ObjectReference(Database context, string id)
        {
            Context = context;
            _id = id;
        }
    }

    public class ObjectReference<T> : ObjectReference
    {
        public T Object { get { return _object; } }

        private readonly T _object;

        internal ObjectReference(Database context, string id, T obj)
            : base(context, id)
        {
            _object = obj;
        }

        public void Update()
        {
            Context.Update(this);
        }

        public void Delete()
        {
            Context.Delete(this);
        }
    }
}
