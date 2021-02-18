﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Audiochan.Core.Common.Models;
using Audiochan.Core.Common.Options;
using Audiochan.Core.Entities;
using Audiochan.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Audiochan.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly IDateTimeService _dateTimeService;
        private readonly UserManager<User> _userManager;

        public TokenService(IOptions<JwtOptions> jwtOptions, UserManager<User> userManager, 
            IDateTimeService dateTimeService)
        {
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
            _dateTimeService = dateTimeService;
        }

        public async Task<(string, long)> GenerateAccessToken(User user)
        {
            var claims = await GetClaims(user);
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Secret));

            var createdDate = _dateTimeService.Now;
            var expirationDate = createdDate.Add(_jwtOptions.AccessTokenExpiration);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationDate,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return (tokenHandler.WriteToken(token), DateTimeToUnixEpoch(expirationDate));
        }

        private async Task<List<Claim>> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new("name", user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            claims.AddRange(roles.Select(role => new Claim("role", role)));

            return claims;
        }

        public RefreshToken GenerateRefreshToken(string userId)
        {
            using var rng = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                Expiry = _dateTimeService.Now.Add(_jwtOptions.RefreshTokenExpiration),
                Created = _dateTimeService.Now,
                UserId = userId
            };
        }

        public long DateTimeToUnixEpoch(DateTime dateTime)
        {
            return EpochTime.GetIntDate(dateTime.ToUniversalTime());
        }

        public bool IsRefreshTokenValid(RefreshToken existingToken)
        {
            if (existingToken.Revoked != null)
            {
                return false;
            }

            if (existingToken.Expiry <= _dateTimeService.Now)
            {
                return false;
            }

            return true;
        }
    }
}