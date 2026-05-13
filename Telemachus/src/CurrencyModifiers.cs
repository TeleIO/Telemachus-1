namespace Telemachus
{
    // Applies KSP's CurrencyModifierQuery against a nominal cost so the
    // reported value matches what the player will actually be charged
    // after active strategies / mod effects have voted. KSP's
    // {Funding,ResearchAndDevelopment,Reputation}.Add* methods apply the
    // mutation BEFORE firing OnCurrencyModifierQuery, so consumers can't
    // adjust the deduction by listening — they have to pre-query the
    // modifier separately, then pass the adjusted value to the Add call.
    // This helper provides that pre-query step in a single line.
    internal static class CurrencyModifiers
    {
        public static float Funds(float nominal, TransactionReasons reason)
        {
            var q = new CurrencyModifierQuery(reason, nominal, 0f, 0f);
            GameEvents.Modifiers.OnCurrencyModifierQuery.Fire(q);
            return q.GetTotal(Currency.Funds);
        }

        public static float Science(float nominal, TransactionReasons reason)
        {
            var q = new CurrencyModifierQuery(reason, 0f, nominal, 0f);
            GameEvents.Modifiers.OnCurrencyModifierQuery.Fire(q);
            return q.GetTotal(Currency.Science);
        }

        public static float Reputation(float nominal, TransactionReasons reason)
        {
            var q = new CurrencyModifierQuery(reason, 0f, 0f, nominal);
            GameEvents.Modifiers.OnCurrencyModifierQuery.Fire(q);
            return q.GetTotal(Currency.Reputation);
        }
    }
}
