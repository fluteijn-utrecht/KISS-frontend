﻿namespace SdgElasticPoller
{
    public static class Extensions
    {
        public static void CancelSafely(this CancellationTokenSource source)
        {
            try
            {
                source.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }
}