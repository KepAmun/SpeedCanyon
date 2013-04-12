namespace SpeedCanyon
{
    class ParticleSettings
    {
        // Size of particle
        public float maxSize = 0.2f;
    }

    class ParticleExplosionSettings
    {
        // Life of particles
        public int minLife = 1000;
        public int maxLife = 1000;

        // Particles per round
        public int minParticlesPerRound = 600;
        public int maxParticlesPerRound = 600;

        // Round time
        public int minRoundTime = 16;
        public int maxRoundTime = 16;

        // Number of particles
        public int minParticles = 2000;
        public int maxParticles = 2000;
    }
}