﻿using System;
using Abp.Domain.Repositories;
using Yufu.Notify.Entities;
using Yufu.Notify.Enums;
using Yufu.Notify.Services.IServices;
using Yufu.Notify.Shared.App.Dto;

namespace Yufu.Notify.Services
{
  public class AppAppService : NotifyAppServiceBase, IAppAppService
  {
    private readonly IRepository<NotifyApplication> _notifyApplicationRepository;
    private readonly IRepository<NotifyAppConfig> _notifyAppConfigRepository;
    private readonly IRepository<NotifyAppQueue> _notifyAppQueueRepository;

    public AppAppService(
      IRepository<NotifyApplication> notifyApplicationRepository,
      IRepository<NotifyAppConfig> notifyAppConfigRepository,
      IRepository<NotifyAppQueue> notifyAppQueueRepository)
    {
      _notifyApplicationRepository = notifyApplicationRepository;
      _notifyAppConfigRepository = notifyAppConfigRepository;
      _notifyAppQueueRepository = notifyAppQueueRepository;
    }

    public NotifyAppConfig ConfigGet(int applicationId)
    {
      return _notifyAppConfigRepository.FirstOrDefault(a => a.Id == applicationId);
    }

    public void QueueSave(NotifyAppQueue entity)
    {
      _notifyAppQueueRepository.InsertOrUpdate(entity);
    }

    public NotifyAppQueue QueueGet(int id)
    {
      return _notifyAppQueueRepository.FirstOrDefault(id);
    }

    public void QueueDelete(int id)
    {
      _notifyAppQueueRepository.Delete(id);
    }

    public void Push(NotifyAppQueue entity)
    {
      var config = ConfigGet(entity.NotifyApplicationId);

      var result = "";
      switch (entity.PushType)
      {
        case PushType.SoftToken:
        {
          result = PushHelper.SendBySoftTokens(config.Authtoken, config.Email, entity.AppId, entity.Title, entity.Body,
            entity.Platforms,
            entity.List);

          break;
        }
        case PushType.AppId:
        {
          result = PushHelper.SendByAppId(config.Authtoken, config.Email, entity.AppId, entity.Title, entity.Body);
          entity.QueueStatusCode = result;
          break;
        }
        case PushType.UserId:
        {
          result = PushHelper.SendByUserIds(config.Authtoken, config.Email, entity.AppId, entity.Title, entity.Body,
            entity.List);
          entity.QueueStatusCode = result;
          break;
        }
        default:
          {break;}
      }
      if (!string.IsNullOrEmpty(result))
      {
        entity.QueueStatus = QueueStatus.Fail; //默认推送失败
        entity.QueueStatusCode = result;
        if (result == "ok")
        {
          entity.QueueStatus = QueueStatus.Success;
        }
        QueueSave(entity);
      }
    }
  }
}