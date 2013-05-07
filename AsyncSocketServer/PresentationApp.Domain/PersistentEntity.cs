#define INT64_IDS

namespace PresentationApp.Domain
{
    public class PersistentEntity
    {
        #region Constants

        public static readonly
#if INTEGER_IDS
            int
#endif
#if INT64_IDS
 long
#endif
#if GUID_IDS
 Guid
#endif

 DefaultValueForId =
#if INTEGER_IDS
         default(int);
#endif
#if INT64_IDS
 default(long);
#endif
#if GUID_IDS
 default(Guid);
#endif

        #endregion Constants

        #region Attributes

#if INTEGER_IDS
		        private int id;
#endif
#if INT64_IDS
        private long id;
#endif
#if GUID_IDS
        private Guid id;
#endif
        private int version;

        #endregion Attributes

        #region Properties

        /// <summary>
        /// Id of the entity.
        /// </summary>
#if INTEGER_IDS
                public virtual int Id

#endif
#if INT64_IDS

        public virtual long Id
#endif
#if GUID_IDS
        public virtual Guid Id
#endif
        {
            get { return id; }
            set { id = value; }
        }

        // TODO: change this to internal version
        /// <summary>
        /// Version for optimistic concurrency.
        /// </summary>
        public virtual int Version
        {
            get { return version; }
            set { version = value; }
        }

        #endregion Properties

        public virtual PersistentEntity Clone()
        {
            return MemberwiseClone() as PersistentEntity;
        }
    }
}