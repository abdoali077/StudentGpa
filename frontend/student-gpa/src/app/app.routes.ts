
import { Routes } from '@angular/router';

import { MainLayout } from './layout/main-layout/main-layout';
import { Dashboard } from './features/dashboard/dashboard';
import { Students } from './features/students/students';
import { Courses } from './features/courses/courses';
import { Semesters } from './features/semesters/semesters';
import { Enrollments } from './features/enrollments/enrollments';
import { TranscriptComponent } from './features/transcript/transcript';

export const routes: Routes = [
  {
    path: '',
    component: MainLayout,
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        component: Dashboard
      },
      {
        path: 'students',
        component: Students
      },
      {
        path: 'courses',
        component: Courses
      },
      {
        path: 'semesters',
        component: Semesters
      },
      {
        path: 'enrollments',
        component: Enrollments
      },
      {
        path: 'transcript',
        component: TranscriptComponent
      }
    ]
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
