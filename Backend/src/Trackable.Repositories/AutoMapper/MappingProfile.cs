﻿using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Trackable.EntityFramework;
using Trackable.Models;
using Trackable.Repositories.AutoMapper;
using Trackable.Repositories.Helpers;

namespace Trackable.Repositories
{
    class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Configuration Mappings
            CreateMap<ConfigurationData, Configuration>()
                .ConvertUsing(dest => new Configuration(dest.Key1, dest.Key2, dest.Description, dest.Value));

            CreateMap<Configuration, ConfigurationData>()
                .ForMember(dest => dest.Key1, opt => opt.MapFrom(src => src.Namespace))
                .ForMember(dest => dest.Key2, opt => opt.MapFrom(src => src.Key))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.SerializedValue))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));

            // Role Mappings
            CreateMap<RoleData, Role>();
            CreateMap<Role, RoleData>();

            // TrackingDevice Mappings
            CreateMap<TrackingDeviceData, TrackingDevice>()
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.Asset.Id));

            CreateMap<TrackingDevice, TrackingDeviceData>()
                .ForMember(dest => dest.Asset, opt => opt.ResolveUsing<TrackingDeviceAssetResolver>());

            // Location Mappings
            CreateMap<LocationData, Location>()
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude.Value))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude.Value))
                .ForMember(dest => dest.InterestLevel, opt =>
                {
                    opt.MapFrom(src => (InterestLevel)(src.InterestLevel ?? 0));
                });

            CreateMap<Location, LocationData>()
                .ForMember(dest => dest.InterestLevel, opt => opt.MapFrom(src => (int)src.InterestLevel))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => GeographyHelper.CreateDbPoint(src)));


            // GeoFence Mappings
            CreateMap<GeoFenceData, GeoFence>()
                .ForMember(dest => dest.FenceType, opt => opt.MapFrom(src => (FenceType)src.FenceType))
                .ForMember(dest => dest.Cooldown, opt => opt.MapFrom(src => src.CooldownInMinutes))
                .ForMember(dest => dest.EmailsToNotify, opt => opt.MapFrom(src => src.Emails.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)))
                .ForMember(dest => dest.AssetIds, opt => opt.MapFrom(src => src.AssetDatas.Select(a => a.Id)))
                .ForMember(dest => dest.FencePolygon, opt => opt.MapFrom(src => GeographyHelper.FromPolygon(src.Polygon)));

            CreateMap<GeoFence, GeoFenceData>()
                .ForMember(dest => dest.FenceType, opt => opt.MapFrom(src => (int)src.FenceType))
                .ForMember(dest => dest.CooldownInMinutes, opt => opt.MapFrom(src => src.Cooldown))
                .ForMember(dest => dest.Emails, opt => opt.MapFrom(src => src.EmailsToNotify == null ? string.Empty : string.Join(",", src.EmailsToNotify)))
                .ForMember(dest => dest.Polygon, opt => opt.MapFrom(src => GeographyHelper.CreatePolygon(src.FencePolygon)));

            // GeoFence update Mappings
            CreateMap<GeoFenceUpdateData, GeoFenceUpdate>()
                .ForMember(dest => dest.NotificationStatus, opt => opt.MapFrom(src => (NotificationStatus)src.Status))
                .ForMember(dest => dest.GeoFenceId, opt => opt.MapFrom(src => src.GeoFenceDataId))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetDataId))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.CreatedAtTimeUtc));

            CreateMap<GeoFenceUpdate, GeoFenceUpdateData>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.NotificationStatus))
                .ForMember(dest => dest.GeoFenceDataId, opt => opt.MapFrom(src => src.GeoFenceId))
                .ForMember(dest => dest.AssetDataId, opt => opt.MapFrom(src => src.AssetId));

            // Tracking point Mappings
            CreateMap<TrackingPointData, TrackingPoint>()
                .ForMember(dest => dest.CreatedAtUtc, opt => opt.MapFrom(src => src.CreatedAtTimeUtc))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Location.Longitude));

            CreateMap<TrackingPoint, TrackingPointData>()
               .ForMember(dest => dest.CreatedAtTimeUtc, opt => opt.MapFrom(src => src.CreatedAtUtc))
               .ForMember(dest => dest.Location, opt => opt.MapFrom(src => GeographyHelper.CreateDbPoint(src)));

            //AssetProperties Mappings
            CreateMap<AssetPropertiesData, AssetProperties>();
            CreateMap<AssetProperties, AssetPropertiesData>();

            // Asset Mappings
            CreateMap<AssetData, Asset>()
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => (AssetType)src.AssetType))
                .ForMember(dest => dest.TrackingDeviceId,
                    opt => opt.MapFrom(src => src.TrackingDevice == null ? null : src.TrackingDevice.Id))
                .ForMember(dest => dest.TrackingDeviceName,
                    opt => opt.MapFrom(src => src.TrackingDevice == null ? null : src.TrackingDevice.Name));

            CreateMap<Asset, AssetData>()
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => (int)src.AssetType));

            // Trip leg mapping
            CreateMap<TripLegData, TripLeg>()
               .ForMember(dest => dest.TripId, opt => opt.MapFrom(src => src.TripDataId))
               .ForMember(dest => dest.Route, opt => opt.MapFrom(src => GeographyHelper.FromDbLine(src.Route)));

            CreateMap<TripLeg, TripLegData>()
               .ForMember(dest => dest.Route, opt => opt.MapFrom(src => GeographyHelper.CreateDbLine(src.Route)));

            // Trip mapping
            CreateMap<TripData, Trip>()
                .ForMember(dest => dest.TripLegs, opt => opt.MapFrom(src => src.TripLegDatas));

            CreateMap<Trip, TripData>()
                .ForMember(dest => dest.TripLegDatas, opt => opt.MapFrom(src => src.TripLegs));

            // User Mapping
            CreateMap<UserData, User>();
            CreateMap<User, UserData>();

            // Deployment Id Mapping
            CreateMap<DeploymentIdData, DeploymentId>();
            CreateMap<DeploymentId, DeploymentIdData>();

            // Jwt Tokens
            CreateMap<TokenData, JwtToken>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User == null ? default(Guid?) : src.User.Id))
                .ForMember(dest => dest.TrackingDeviceId, opt => opt.MapFrom(src => src.TrackingDevice == null ? null : src.TrackingDevice.Id))
                .ForMember(dest => dest.Claims, opt => opt.MapFrom(src => JsonConvert.DeserializeObject<Dictionary<string, string>>(src.Value).Select(d => new Claim(d.Key, d.Value))));
            CreateMap<JwtToken, TokenData>()
                .ForMember(dest => dest.User, opt => opt.ResolveUsing<TokenUserResolver>())
                .ForMember(dest => dest.TrackingDevice, opt => opt.ResolveUsing<TokenTrackingDeviceResolver>())
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Claims.ToDictionary(c => c.Type, c => c.Value))));
        }
    }
}
