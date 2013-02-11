namespace SpeedCanyon
{
    class ParticleSettings
    {
        // Size of particle
        public int maxSize = 2;
    }

    class ParticleExplosionSettings
    {
        // Life of particles
        public int minLife = 500;//1000;
        public int maxLife = 1000;//2000;

        // Particles per round
        public int minParticlesPerRound = 100;
        public int maxParticlesPerRound = 600;

        // Round time
        public int minRoundTime = 16;
        public int maxRoundTime = 24;//50;

        // Number of particles
        public int minParticles = 2000;
        public int maxParticles = 3000;
    }
}