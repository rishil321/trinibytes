import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CaribbeanJobsPostsComponent } from './caribbean-jobs-posts.component';

describe('CaribbeanjobspostsComponent', () => {
  let component: CaribbeanJobsPostsComponent;
  let fixture: ComponentFixture<CaribbeanJobsPostsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ CaribbeanJobsPostsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(CaribbeanJobsPostsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
