﻿using Raje.DL.DB.Admin;
using Raje.DL.Request.Base;
using Raje.DL.Request.Identity;
using Raje.DL.Services.BLL.Base;
using System.Threading.Tasks;

namespace Raje.DL.Services.BLL.Identity
{
    public interface IResetPasswordService : IDependencyInjectionService
    {
        Task<string> SendEmail(BaseResetPasswordRequest request);

        void ResetPassword(ResetPasswordRequest request);

        /// <summary>
        /// Reset password to a specifc user (Do not use to reset collaborator user)
        /// </summary>
        /// <param name="request"></param>
        void ValidateVerificationCode(ValidateVerificationCodeRequest request);

        Task SendUserEmail(User model);

        string EncryptedToken(string passwordHash);
    }
}