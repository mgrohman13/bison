using ClassLibrary1.Pieces.Behavior.Combat;

namespace ClassLibrary1.Pieces.Behavior
{
    public interface IMissileSilo : IBehavior
    {
        public IAttacker.Values Attack { get; }
        public Attack SampleAttack
        {
            get
            {
                Attack sample = new(Piece, Attack);
                if (Online)
                    sample.StartTurn();
                return sample;
            }
        }
        public bool Producing { get; }
        public int NumMissiles { get; }

        public bool Online => NumMissiles > 0;

        public double GetAttack(IKillable killable);
        public bool Fire(IKillable killable);
    }
}
