namespace Faryma.Composer.Contracts.Api
{
    public static class Globals
    {
        public const string IdempotencyKey = "Idempotency-Key";

        /// <summary>
        /// Максимальная длительность трека 15 мин
        /// </summary>
        public const int MaxTrackDurationSeconds = 60 * 15;
    }
}
