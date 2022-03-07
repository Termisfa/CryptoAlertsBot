﻿using CryptoAlertsBot.ApiHandler;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace CryptoAlertsBot.Discord.Preconditions
{
    public class IsCoinChannelAttribute : ParameterPreconditionAttribute
    {
        public IsCoinChannelAttribute() { }

        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, IParameterInfo parameter, object value, IServiceProvider services)
        {
            try
            {
                ConstantsHandler constantsHandler = (ConstantsHandler)services.GetService(typeof(ConstantsHandler));

                ulong? channelCategoryId = ((SocketTextChannel)value).CategoryId;

                if (context.User is SocketGuildUser gUser)
                {
                    string dbCategoryChannelId = constantsHandler.GetConstant(ConstantsNames.DB_CATEGORY_CHANNEL_ID);

                    if (ulong.Parse(dbCategoryChannelId) == channelCategoryId)
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    else
                        return Task.FromResult(PreconditionResult.FromError($"El canal introducido debe ser una moneda."));
                }
                else
                    return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
