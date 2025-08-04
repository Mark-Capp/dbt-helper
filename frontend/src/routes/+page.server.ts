import { env } from '$env/dynamic/private';

export const load = async () => {
    return {
        post: env.API_URI
    }
}