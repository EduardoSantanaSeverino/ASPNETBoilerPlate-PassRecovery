import { NgModule } from '@angular/core';
import { PreloadAllModules, RouterModule, Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';

const routes: Routes = [
    {
        path: 'login',
        loadChildren: './pages/authentication/login/login.module#LoginModule',
    },
    {
        path: 'register',
        loadChildren: './pages/authentication/register/register.module#RegisterModule',
    },
    {
        path: 'validate/:instaAccountId',
        loadChildren: './pages/authentication/validate/validate.module#ValidateModule',
    },
    {
        path: 'forgot-password',
        loadChildren: './pages/authentication/forgot-password/forgot-password.module#ForgotPasswordModule',
    },
    {
        path: 'reset-password',
        loadChildren: './pages/authentication/reset-password/reset-password.module#ResetPasswordModule',
    },
    {
        path: '',
        component: LayoutComponent,
        children: [
            {
                path: '',
                redirectTo: 'instaaccounts',
                pathMatch: 'full',
                //canActivate: [AppRouteGuard]
            },
            {
                path: 'instaaccounts',
                loadChildren: './pages/instaaccounts/instaaccounts.module#InstaaccountsModule',
                //canActivate: [AppRouteGuard]
            },
            {
                path: 'campaigns',
                loadChildren: './pages/campaigns/campaigns.module#CampaignsModule',
                //canActivate: [AppRouteGuard]
            },
            {
                path: 'users',
                loadChildren: './pages/users/users.module#UsersModule',
            },
        ]
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes, {
        initialNavigation: 'enabled',
        preloadingStrategy: PreloadAllModules,
        scrollPositionRestoration: 'enabled',
        anchorScrolling: 'enabled'
    })],
    exports: [RouterModule]
})
export class AppRoutingModule { }

