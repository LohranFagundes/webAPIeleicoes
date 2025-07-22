using AutoMapper;
using ElectionApi.Net.DTOs;
using ElectionApi.Net.Models;
using System.Text.Json;

namespace ElectionApi.Net.Services;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Election mappings
        CreateMap<CreateElectionDto, Election>();
        CreateMap<UpdateElectionDto, Election>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Election, ElectionResponseDto>();

        // Voter mappings
        CreateMap<CreateVoterDto, Voter>()
            .ForMember(dest => dest.Password, opt => opt.Ignore()); // Password will be hashed separately
        CreateMap<UpdateVoterDto, Voter>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
        CreateMap<Voter, VoterResponseDto>();

        // Vote mappings
        CreateMap<CastVoteDto, Vote>();
        CreateMap<Vote, VoteResponseDto>();

        // Admin mappings
        CreateMap<Admin, UserDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
            .ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.Permissions));

        // Voter to UserDto mapping
        CreateMap<Voter, UserDto>()
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf))
            .ForMember(dest => dest.VoteWeight, opt => opt.MapFrom(src => src.VoteWeight));
    }
}