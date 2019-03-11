using Sitecore.Commerce.Core;
using Sitecore.Commerce.Engine.Connect.Pipelines.Arguments;
using Sitecore.Commerce.Engine.Connect.Pipelines.Carts;
using Sitecore.Commerce.Pipelines;
using Sitecore.Commerce.Plugin.Carts;
using Sitecore.Commerce.Services;
using Sitecore.Commerce.Services.Carts;
using Sitecore.ContentSearch.Utilities;
using Sitecore.Diagnostics;
using System;
using System.Linq;

namespace Sitecore.Support.Commerce.Engine.Connect.Pipelines.Carts
{
  public class LoadCart : CartProcessor
  {
    public override void Process(ServicePipelineArgs args)
    {
      ValidateArguments(args, out LoadCartRequest request, out CartResult result);
      try
      {
        Assert.IsNotNullOrEmpty(request.UserId, "request.Cart.UserId");
        bool num = request is LoadCartByNameRequest;
        string text = request.CartId;
        if (num && !string.IsNullOrEmpty(((LoadCartByNameRequest)request).CartName))
        {
          text = ((LoadCartByNameRequest)request).CartName;
        }
        Assert.IsNotNullOrEmpty(text, "cartId");
        string cartId = text;
        if (num)
        {
          cartId = text + request.UserId + request.Shop.Name;
        }
        string customerId = string.Empty;
        if (!string.IsNullOrEmpty(request.UserId))
        {
          customerId = ((!string.IsNullOrEmpty(customerId)) ? customerId : (request.UserId.StartsWith("Entity-Customer-") ? request.UserId : ""));
        }
        Cart cart = GetCart(request.UserId, request.Shop.Name, cartId, customerId, args.Request.CurrencyCode);
        if (cart != null)
        {
          result.Cart = TranslateCartToEntity(cart, result);
          MessagesComponent messagesComponent = cart.Components.OfType<MessagesComponent>().FirstOrDefault<MessagesComponent>();
          if (messagesComponent != null)
            messagesComponent.Messages.Where<MessageModel>((Func<MessageModel, bool>)(m => m.Code.Equals("error", StringComparison.OrdinalIgnoreCase))).ForEach<MessageModel>((Action<MessageModel>)(m => result.SystemMessages.Add(CreateSystemMessage(m.Text))));
        }
        else
        {
          result.Success = false;
        }
      }
      catch (ArgumentException ex)
      {
        result.Success = false;
        result.SystemMessages.Add(CreateSystemMessage(ex));
      }
      catch (AggregateException ex2)
      {
        result.Success = false;
        result.SystemMessages.Add(CreateSystemMessage(ex2));
      }
      base.Process(args);
    }

    protected static SystemMessage CreateSystemMessage(Exception ex)
    {
      return new SystemMessage
      {
        Message = ex.ToString()
      };
    }

    protected static SystemMessage CreateSystemMessage(string message)
    {
      return new SystemMessage
      {
        Message = message
      };
    }

    protected static void ValidateArguments<TRequest, TResult>(ServicePipelineArgs args, out TRequest request, out TResult result) where TRequest : ServiceProviderRequest where TResult : ServiceProviderResult
    {
      Assert.ArgumentNotNull(args, "args");
      Assert.ArgumentNotNull(args.Request, "args.Request");
      Assert.ArgumentNotNull(args.Request.RequestContext, "args.Request.RequestContext");
      Assert.ArgumentNotNull(args.Result, "args.Result");
      request = (args.Request as TRequest);
      result = (args.Result as TResult);
      Assert.IsNotNull(request, "The parameter args.Request was not of the expected type.  Expected {0}.  Actual {1}.", typeof(TRequest).Name, args.Request.GetType().Name);
      Assert.IsNotNull(result, "The parameter args.Result was not of the expected type.  Expected {0}.  Actual {1}.", typeof(TResult).Name, args.Result.GetType().Name);
    }
  }
}