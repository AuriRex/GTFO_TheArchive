namespace TheArchive.Interfaces
{
    public interface IInitCondition
    {
        /// <summary>
        /// Works in tandem with <see cref="IInitializable"/> in order to restrict when it's supposed to init.
        /// </summary>
        /// <returns>Whether to call <see cref="IInitializable.Init"/> or not</returns>
        public bool InitCondition();
    }
}
